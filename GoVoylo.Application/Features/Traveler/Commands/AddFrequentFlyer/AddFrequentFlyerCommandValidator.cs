using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.AddFrequentFlyer
{
    public class AddFrequentFlyerCommandValidator : AbstractValidator<AddFrequentFlyerCommand>
    {
        public AddFrequentFlyerCommandValidator()
        {
            RuleFor(x => x.AirlineCode)
                .NotEmpty()
                .Length(2, 3)
                .WithMessage("Airline code must be a 2-3 letter IATA code.");

            RuleFor(x => x.MembershipNumber).NotEmpty().MaximumLength(30);
        }
    }
}
