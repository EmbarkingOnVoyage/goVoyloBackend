using FluentValidation;

namespace GoVoylo.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpCommandValidator()
        {
            RuleFor(x => x.VerificationToken)
                .NotEmpty()
                .WithMessage("Verification token is required.");

            RuleFor(x => x.Otp)
                .NotEmpty()
                .WithMessage("OTP is required.")
                .Length(6)
                .WithMessage("OTP must be 6 digits.")
                .Matches(@"^\d{6}$")
                .WithMessage("OTP must contain only numbers.");
        }
    }
}
