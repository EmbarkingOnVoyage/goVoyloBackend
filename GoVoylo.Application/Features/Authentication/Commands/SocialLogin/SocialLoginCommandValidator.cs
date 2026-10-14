using FluentValidation;

namespace GoVoylo.Application.Features.Authentication.Commands.SocialLogin
{
    public class SocialLoginCommandValidator : AbstractValidator<SocialLoginCommand>
    {
        public SocialLoginCommandValidator()
        {
            RuleFor(x => x.Provider)
                .NotEmpty()
                .Must(p => p is "google" or "apple" or "facebook")
                .WithMessage("Provider must be google, apple or facebook.");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Sign-in token is required.");

            RuleFor(x => x.FirstName).MaximumLength(100);
            RuleFor(x => x.LastName).MaximumLength(100);
        }
    }
}
