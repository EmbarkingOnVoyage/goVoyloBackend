using MediatR;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Application.Interfaces;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public class VerifyRazorpayPaymentCommandHandler : IRequestHandler<VerifyRazorpayPaymentCommand, PaymentResponseDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRazorpayClient _razorpayClient;

    public VerifyRazorpayPaymentCommandHandler(IPaymentRepository paymentRepository, IRazorpayClient razorpayClient)
    {
        _paymentRepository = paymentRepository;
        _razorpayClient = razorpayClient;
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

        return new PaymentResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency,
            payment.PaymentStatus,
            payment.CreatedAt);
    }
}
