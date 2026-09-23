using FluentValidation;

namespace GoVoylo.Application.Features.Payments.Commands.CreateRazorpayOrder;

public class CreateRazorpayOrderCommandValidator : AbstractValidator<CreateRazorpayOrderCommand>
{
    public CreateRazorpayOrderCommandValidator()
    {
        RuleFor(x => x.BookingReference).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3).WithMessage("Invalid ISO currency code.");
        RuleFor(x => x.SourceClient).NotEmpty();
    }
}
