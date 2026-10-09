using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateContactDetails
{
    public class UpdateContactDetailsCommandValidator : AbstractValidator<UpdateContactDetailsCommand>
    {
        public UpdateContactDetailsCommandValidator()
        {
            // Same rule as the app's Personal details: a 10-digit Indian mobile.
            RuleFor(x => x.Phone)
                .Matches(@"^[6-9]\d{9}$")
                .WithMessage("Please enter a valid 10-digit mobile number.")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Phone) || !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Please enter an email address or a mobile number.");

            RuleFor(x => x.Email)
                .EmailAddress().MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
