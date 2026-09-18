using GoVoylo.Application.Features.Airports.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.GetAirport
{
    public record GetAirportQuery(string IataCode) : IRequest<AirportDto>;
}
