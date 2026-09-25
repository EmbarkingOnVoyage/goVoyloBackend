using MediatR;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Application.Interfaces;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public class VerifyRazorpayPaymentCommandHandler : IRequestHandler<VerifyRazorpayPaymentCommand, PaymentResponseDto>
{
    // Flyshop's own Status_Id convention (Air_Ticketing docs): 33-Block is the only
    // state AddPayment + Book_Ticket should ever run against here — a booking that's
    // already ticketed or already failed has nothing left to do.
    private const string StatusBlocked = "33";

    private readonly IPaymentRepository _paymentRepository;
    private readonly IRazorpayClient _razorpayClient;
    private readonly ITripBookingRepository _tripBookingRepository;
    private readonly IFlightSupplierClient _supplierClient;

    public VerifyRazorpayPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IRazorpayClient razorpayClient,
        ITripBookingRepository tripBookingRepository,
        IFlightSupplierClient supplierClient)
    {
        _paymentRepository = paymentRepository;
        _razorpayClient = razorpayClient;
        _tripBookingRepository = tripBookingRepository;
        _supplierClient = supplierClient;
    }

    public async Task<PaymentResponseDto> Handle(VerifyRazorpayPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByGatewayOrderIdAsync(request.RazorpayOrderId, cancellationToken)
            ?? throw new NotFoundException($"No payment found for Razorpay order '{request.RazorpayOrderId}'.");

        var isValid = _razorpayClient.VerifySignature(
            request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

        if (!isValid)
        {
            payment.MarkAsFailed();
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            throw new BusinessRuleException("invalid_payment_signature", "Payment signature verification failed.");
        }

        payment.MarkAsSucceeded(request.RazorpayPaymentId);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        // The customer's Razorpay payment has genuinely succeeded by this point.
        // payment.BookingReference is the same Flyshop Booking_RefNo the hold was
        // created under (TravelerDetailsScreen.handlePayNow passes the real
        // CreateBooking result's bookingRefNo, not a synthetic client-side id), so
        // this lookup is never ambiguous between BookingPayment and TripBooking.
        var booking = await _tripBookingRepository.GetByBookingRefNoAsync(payment.BookingReference, cancellationToken);

        if (booking != null && booking.StatusId == StatusBlocked)
        {
            // Deliberately not wrapped in try/catch, unlike the best-effort
            // email/persist paths in CreateBookingCommandHandler: the customer has
            // already been charged by this point, so a Flyshop failure here must
            // surface as a real error (needing manual follow-up — refund or retry)
            // rather than being swallowed into a false "booking confirmed" response.
            await _supplierClient.AddPaymentAsync(booking.BookingRefNo, payment.Id.ToString(), cancellationToken);

            var ticket = await _supplierClient.BookTicketAsync(booking.BookingRefNo, cancellationToken);

            booking.MarkTicketed(ticket.StatusId, ticket.AirlinePnr, ticket.RecordLocator);
            await _tripBookingRepository.UpdateAsync(booking, cancellationToken);
        }

        return new PaymentResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency,
            payment.PaymentStatus,
            payment.CreatedAt);
    }
}
