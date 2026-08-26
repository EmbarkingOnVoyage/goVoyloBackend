using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateTravelerPreferences
{
    public class UpdateTravelerPreferencesCommandValidator
        : AbstractValidator<UpdateTravelerPreferencesCommand>
    {
        private static readonly string[] ValidMeals = { "vegetarian", "jain", "vegan", "kosher", "non-vegetarian" };
        private static readonly string[] ValidSeats = { "window", "aisle", "middle" };
        private static readonly string[] ValidSsrCodes = { "wchr", "meda", "inft", "dbml", "bbml" };

        public UpdateTravelerPreferencesCommandValidator()
        {
            RuleFor(x => x.MealPreference)
                .Must(m => ValidMeals.Contains(m!.ToLowerInvariant()))
                .WithMessage("Unsupported meal preference.")
                .When(x => x.MealPreference != null);

            RuleFor(x => x.SeatPreference)
                .Must(s => ValidSeats.Contains(s!.ToLowerInvariant()))
                .WithMessage("Seat preference must be window, aisle, or middle.")
                .When(x => x.SeatPreference != null);

            RuleForEach(x => x.SpecialAssistance)
                .Must(code => ValidSsrCodes.Contains(code.ToLowerInvariant()))
                .WithMessage("Unsupported special assistance code.");
        }
    }
}
