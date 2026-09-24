using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Commands.CancelTripBooking
{
    public class CancelTripBookingCommandValidator : AbstractValidator<CancelTripBookingCommand>
    {
        public CancelTripBookingCommandValidator()
        {
            RuleFor(x => x.TripBookingId).NotEmpty();
        }
    }
}
