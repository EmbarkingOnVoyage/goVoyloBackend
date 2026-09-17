using GoVoylo.Application.Features.Airports.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.SearchAirports
{
    public record SearchAirportsQuery(string Query) : IRequest<IReadOnlyList<AirportDto>>;
}
