using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Commands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.Legs).NotEmpty();
            RuleFor(x => x.Travelers).NotEmpty();
            RuleFor(x => x.PassengerMobile).NotEmpty();
            RuleFor(x => x.PassengerEmail).NotEmpty().EmailAddress();

            RuleForEach(x => x.Legs).ChildRules(leg =>
            {
                leg.RuleFor(l => l.OfferId).NotEmpty();
            });

            RuleForEach(x => x.Travelers).ChildRules(traveler =>
            {
                traveler.RuleFor(t => t.FirstName).NotEmpty();
                traveler.RuleFor(t => t.LastName).NotEmpty();
            });
        }
    }
}
