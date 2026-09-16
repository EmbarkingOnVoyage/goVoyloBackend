using FluentValidation;

namespace GoVoylo.Application.Features.Authentication.Commands.LoginWithOtp
{
    public class LoginWithOtpCommandValidator : AbstractValidator<LoginWithOtpCommand>
    {
        public LoginWithOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Invalid email address.");

            RuleFor(x => x.VerificationToken)
                .NotEmpty()
                .WithMessage("Verification token is required.");

            RuleFor(x => x.Otp)
                .NotEmpty()
                .Length(6)
                .WithMessage("OTP must be 6 digits.");
        }
    }
}
