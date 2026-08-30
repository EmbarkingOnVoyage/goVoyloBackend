using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    // UserId is optional — flight search itself doesn't require authentication, but a
    // caller who is logged in gets their searched airports tracked for recent-search recall.
    public record SearchFlightsQuery(FlightSearchRequestDto Request, Guid? UserId = null)
        : IRequest<FlightSearchResponseDto>;
}
