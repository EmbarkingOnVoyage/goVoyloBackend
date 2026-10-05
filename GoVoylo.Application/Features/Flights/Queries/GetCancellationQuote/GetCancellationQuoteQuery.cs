using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetCancellationQuote
{
    // What cancelling would refund, without cancelling. LegIndex null = whole booking,
    // same meaning as CancelTripBookingCommand.LegIndex.
    public record GetCancellationQuoteQuery(Guid TripBookingId, Guid UserId, int? LegIndex = null)
        : IRequest<CancellationQuoteDto>;
}
