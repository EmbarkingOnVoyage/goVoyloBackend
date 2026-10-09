using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.AddTraveler
{
    public class AddTravelerCommandValidator : AbstractValidator<AddTravelerCommand>
    {
        private static readonly string[] ValidTypes = { "adult", "child", "infant" };

        public AddTravelerCommandValidator()
        {
            RuleFor(x => x.TravelerType)
                .NotEmpty()
                .Must(t => ValidTypes.Contains(t.ToLowerInvariant()))
                .WithMessage("Traveler type must be adult, child, or infant.");

            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Date of birth cannot be in the future.");

            RuleFor(x => x.Email)
                .EmailAddress().MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.Phone)
                .Matches(@"^\d{6,15}$").WithMessage("Mobile number must be 6 to 15 digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));
            RuleFor(x => x.PhoneCountryCode)
                .Matches(@"^\+\d{1,4}$").WithMessage("Country code must look like +91.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneCountryCode));

            // Infant <2, Child 2-12, Adult >12 (GV-TRV-BE-002)
            RuleFor(x => x)
                .Must(HaveConsistentAgeForType)
                .WithMessage("Traveler type does not match the given date of birth.")
                .When(x => ValidTypes.Contains(x.TravelerType.ToLowerInvariant()));
        }

        private static bool HaveConsistentAgeForType(AddTravelerCommand command)
        {
            var ageYears = (DateTime.UtcNow.Date - command.DateOfBirth.Date).TotalDays / 365.25;

            return command.TravelerType.ToLowerInvariant() switch
            {
                "infant" => ageYears < 2,
                "child" => ageYears is >= 2 and <= 12,
                "adult" => ageYears > 12,
                _ => true
            };
        }
    }
}
