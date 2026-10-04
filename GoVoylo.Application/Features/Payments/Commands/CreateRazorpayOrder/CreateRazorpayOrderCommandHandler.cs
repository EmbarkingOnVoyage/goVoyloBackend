using MediatR;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Application.Interfaces;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.CreateRazorpayOrder;

public class CreateRazorpayOrderCommandHandler : IRequestHandler<CreateRazorpayOrderCommand, RazorpayOrderResponseDto>
{
    // Only a held booking is waiting for payment — see VerifyRazorpayPaymentCommandHandler.
    private const string StatusBlocked = "33";

    private readonly IPaymentRepository _paymentRepository;
    private readonly IRazorpayClient _razorpayClient;
    private readonly ITripBookingRepository _tripBookingRepository;

    public CreateRazorpayOrderCommandHandler(
        IPaymentRepository paymentRepository,
        IRazorpayClient razorpayClient,
        ITripBookingRepository tripBookingRepository)
    {
        _paymentRepository = paymentRepository;
        _razorpayClient = razorpayClient;
        _tripBookingRepository = tripBookingRepository;
    }

    public async Task<RazorpayOrderResponseDto> Handle(CreateRazorpayOrderCommand request, CancellationToken cancellationToken)
    {
        // The amount comes from the app, so it's checked against the booking we
        // recorded at hold time: the caller must own a booking that is still held,
        // and can't be charged less than its supplier-confirmed total (which already
        // includes any priced SSRs for Tripjack). Paying a little more than that is
        // allowed because Flyshop's recorded total doesn't include add-ons the app
        // prices on its side.
        var booking = await _tripBookingRepository.GetByBookingRefNoAsync(request.BookingReference, cancellationToken);
        if (booking == null || booking.UserId != request.UserId)
        {
            throw new NotFoundException($"No booking found for reference '{request.BookingReference}'.");
        }

        if (booking.StatusId != StatusBlocked)
        {
            throw new BusinessRuleException("booking_not_payable", "This booking is no longer awaiting payment.");
        }

        if (!string.Equals(request.Currency, booking.CurrencyCode, StringComparison.OrdinalIgnoreCase)
            || Math.Round(request.Amount) < Math.Round(booking.TotalAmount))
        {
            throw new BusinessRuleException("payment_amount_mismatch", "The payment amount doesn't match this booking.");
        }

        var order = await _razorpayClient.CreateOrderAsync(
            request.Amount, request.Currency, request.BookingReference, cancellationToken);

        // A retried "Pay Now" tap reuses the same booking reference — keep the
        // one BookingPayment row per booking and just point it at the fresh
        // Razorpay order instead of accumulating duplicate rows.
        var payment = await _paymentRepository.GetByReferenceAsync(request.BookingReference);
        if (payment == null)
        {
            payment = new BookingPayment(request.BookingReference, request.Amount, request.Currency);
            payment.SetGatewayOrder(order.OrderId);
            await _paymentRepository.SaveAsync(payment);
        }
        else
        {
            payment.SetGatewayOrder(order.OrderId);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }

        return new RazorpayOrderResponseDto(
            order.OrderId, order.Amount, order.Currency, _razorpayClient.KeyId, request.BookingReference);
    }
}
