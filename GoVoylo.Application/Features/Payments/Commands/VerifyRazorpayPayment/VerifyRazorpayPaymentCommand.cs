using MediatR;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyRazorpayPayment
{
    public record VerifyRazorpayPaymentCommand(
        string RazorpayOrderId,
        string RazorpayPaymentId,
        string RazorpaySignature
    ) : IRequest<PaymentResponseDto>;
}
