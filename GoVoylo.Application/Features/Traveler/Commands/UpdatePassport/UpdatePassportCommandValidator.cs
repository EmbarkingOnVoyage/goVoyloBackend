using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdatePassport
{
    public class UpdatePassportCommandValidator : AbstractValidator<UpdatePassportCommand>
    {
        public UpdatePassportCommandValidator()
        {
            RuleFor(x => x.PassportNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.IssuingCountry).NotEmpty().MaximumLength(50);

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow.Date)
                .WithMessage("Passport has already expired.");
        }
    }
}
