using FluentValidation;

namespace GoVoylo.Application.Features.Authentication.Commands.StartGuestSession
{
    public class StartGuestSessionCommandValidator : AbstractValidator<StartGuestSessionCommand>
    {
        public StartGuestSessionCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(255)
                .EmailAddress()
                .WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .Matches(@"^[6-9]\d{9}$")
                .WithMessage("Please enter a valid 10-digit mobile number.");
        }
    }
}
