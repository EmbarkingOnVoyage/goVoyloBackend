using GoVoylo.Application.Features.Airports.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.GetPopularAirports
{
    public record GetPopularAirportsQuery : IRequest<IReadOnlyList<AirportDto>>;
}
