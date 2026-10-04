using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetSeatMap
{
    // ItineraryOfferIds: see GetFlightAncillariesQuery.
    public record GetSeatMapQuery(
        Guid OfferId,
        IReadOnlyList<SeatMapTravelerRequestDto> Travelers,
        IReadOnlyList<Guid> ItineraryOfferIds) : IRequest<SeatMapResponseDto>;
}
