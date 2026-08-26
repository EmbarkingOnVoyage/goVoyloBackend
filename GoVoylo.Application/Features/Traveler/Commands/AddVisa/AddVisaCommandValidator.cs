using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.AddVisa
{
    public class AddVisaCommandValidator : AbstractValidator<AddVisaCommand>
    {
        public AddVisaCommandValidator()
        {
            RuleFor(x => x.Country).NotEmpty().MaximumLength(50);
            RuleFor(x => x.VisaNumber).NotEmpty().MaximumLength(30);
            RuleFor(x => x.VisaType).MaximumLength(30);

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow.Date)
                .WithMessage("Visa has already expired.");
        }
    }
}
