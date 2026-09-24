using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Commands.ReleaseHold
{
    public class ReleaseHoldCommandValidator : AbstractValidator<ReleaseHoldCommand>
    {
        public ReleaseHoldCommandValidator()
        {
            RuleFor(x => x.BookingRefNo).NotEmpty();
            RuleFor(x => x.AirlinePnr).NotEmpty();
        }
    }
}
