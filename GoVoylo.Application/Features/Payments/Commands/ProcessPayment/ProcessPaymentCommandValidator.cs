using FluentValidation;

namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3).WithMessage("Invalid ISO Currency code.");
        RuleFor(x => x.PaymentMethodToken).NotEmpty().WithMessage("Payment token is required.");
    }
}
