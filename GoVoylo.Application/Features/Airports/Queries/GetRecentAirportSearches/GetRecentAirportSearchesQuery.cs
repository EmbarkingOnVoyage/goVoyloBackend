using GoVoylo.Application.Features.Airports.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.GetRecentAirportSearches
{
    public record GetRecentAirportSearchesQuery(Guid UserId) : IRequest<IReadOnlyList<AirportDto>>;
}
