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

            // GST fields are all-optional, but a GST invoice needs all three once any
            // one of them is supplied — half a GST detail isn't something Flyshop can
            // do anything useful with.
            When(x => !string.IsNullOrWhiteSpace(x.GstNumber), () =>
            {
                RuleFor(x => x.GstHolderName).NotEmpty()
                    .WithMessage("GST holder name is required when a GST number is provided.");
                RuleFor(x => x.GstAddress).NotEmpty()
                    .WithMessage("GST address is required when a GST number is provided.");
            });
        }
    }
}
