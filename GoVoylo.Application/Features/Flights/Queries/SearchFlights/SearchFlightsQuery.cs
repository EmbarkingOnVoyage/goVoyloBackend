using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public record SearchFlightsQuery(FlightSearchRequestDto Request) : IRequest<FlightSearchResponseDto>;
}
