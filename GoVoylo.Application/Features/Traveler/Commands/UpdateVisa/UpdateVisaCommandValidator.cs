using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateVisa
{
    public class UpdateVisaCommandValidator : AbstractValidator<UpdateVisaCommand>
    {
        public UpdateVisaCommandValidator()
        {
            RuleFor(x => x.VisaNumber).NotEmpty().MaximumLength(30);
            RuleFor(x => x.VisaType).MaximumLength(30);

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow.Date)
                .WithMessage("Visa has already expired.");
        }
    }
}
