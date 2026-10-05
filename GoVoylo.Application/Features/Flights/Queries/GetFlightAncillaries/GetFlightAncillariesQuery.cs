using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries
{
    // ItineraryOfferIds: every leg of the trip in display order (see ItineraryReprice),
    // or empty when the offer is the whole trip.
    // FareIds: the fare picked for each leg (same order as ItineraryOfferIds, or a
    // single entry for a single offer) — see ItineraryReprice.
    public record GetFlightAncillariesQuery(
        Guid OfferId, IReadOnlyList<Guid> ItineraryOfferIds, IReadOnlyList<string?>? FareIds = null)
        : IRequest<FlightAncillariesResponseDto>;
}
