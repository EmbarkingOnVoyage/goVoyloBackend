using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.UpdatePreferences
{
    public class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
    {
        private static readonly string[] SupportedLanguages = { "en", "hi" };

        public UpdatePreferencesCommandValidator()
        {
            RuleFor(x => x.Language)
                .NotEmpty()
                .Must(l => SupportedLanguages.Contains(l.ToLowerInvariant()))
                .WithMessage("Unsupported language.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be a 3-letter ISO code.");
        }
    }
}
