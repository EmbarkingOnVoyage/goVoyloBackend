using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Commands.CancelBooking
{
    public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(x => x.RefNo).NotEmpty();
            RuleFor(x => x.AirlinePnr).NotEmpty();
            RuleFor(x => x.CancelCode).NotEmpty();
            RuleFor(x => x.ReqRemarks).NotEmpty();
            RuleFor(x => x.Segments).NotEmpty();

            // 0-Normal Cancel/1-Full Refund/2-No Show — see Air_TicketCancellation's
            // own docs. The specific CancelCode-per-type table isn't re-validated
            // here; an invalid combination is something Flyshop's own API rejects.
            RuleFor(x => x.CancellationType).InclusiveBetween(0, 2);

            RuleForEach(x => x.Segments).ChildRules(segment =>
            {
                segment.RuleFor(s => s.FlightId).NotEmpty();
                segment.RuleFor(s => s.PassengerId).NotEmpty();
                segment.RuleFor(s => s.SegmentId).NotEmpty();
            });
        }
    }
}
