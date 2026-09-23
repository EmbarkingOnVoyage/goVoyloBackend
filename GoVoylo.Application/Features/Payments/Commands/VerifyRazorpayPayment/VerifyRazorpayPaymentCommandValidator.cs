using FluentValidation;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public class VerifyRazorpayPaymentCommandValidator : AbstractValidator<VerifyRazorpayPaymentCommand>
{
    public VerifyRazorpayPaymentCommandValidator()
    {
        RuleFor(x => x.RazorpayOrderId).NotEmpty();
        RuleFor(x => x.RazorpayPaymentId).NotEmpty();
        RuleFor(x => x.RazorpaySignature).NotEmpty();
    }
}
