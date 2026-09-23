using MediatR;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Application.Interfaces;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.CreateRazorpayOrder;

public class CreateRazorpayOrderCommandHandler : IRequestHandler<CreateRazorpayOrderCommand, RazorpayOrderResponseDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRazorpayClient _razorpayClient;

    public CreateRazorpayOrderCommandHandler(IPaymentRepository paymentRepository, IRazorpayClient razorpayClient)
    {
        _paymentRepository = paymentRepository;
        _razorpayClient = razorpayClient;
    }

    public async Task<RazorpayOrderResponseDto> Handle(CreateRazorpayOrderCommand request, CancellationToken cancellationToken)
    {
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
