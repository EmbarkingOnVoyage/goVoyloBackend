using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateCustomerProfile
{
    public class UpdateCustomerProfileCommandValidator
        : AbstractValidator<UpdateCustomerProfileCommand>
    {
        public UpdateCustomerProfileCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("FirstName is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("LastName is required.")
                .MaximumLength(100);

            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .Matches(@"^\+?[0-9\s\-()]*$")
                .WithMessage("Invalid phone number.")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        }
    }
}
