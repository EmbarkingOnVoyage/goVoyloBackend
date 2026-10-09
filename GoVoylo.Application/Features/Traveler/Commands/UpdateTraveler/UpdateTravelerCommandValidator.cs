using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateTraveler
{
    public class UpdateTravelerCommandValidator : AbstractValidator<UpdateTravelerCommand>
    {
        private static readonly string[] ValidTypes = { "adult", "child", "infant" };

        public UpdateTravelerCommandValidator()
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
        }
    }
}
