using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries
{
    // ItineraryOfferIds: every leg of the trip in display order (see ItineraryReprice),
    // or empty when the offer is the whole trip.
    public record GetFlightAncillariesQuery(Guid OfferId, IReadOnlyList<Guid> ItineraryOfferIds)
        : IRequest<FlightAncillariesResponseDto>;
}
