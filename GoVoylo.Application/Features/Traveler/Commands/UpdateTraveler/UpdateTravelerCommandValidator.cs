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
        }
    }
}
