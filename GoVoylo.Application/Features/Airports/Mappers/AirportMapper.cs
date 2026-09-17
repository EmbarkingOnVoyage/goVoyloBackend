using GoVoylo.Application.Features.Airports.Dtos;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Airports.Mappers
{
    public static class AirportMapper
    {
        public static AirportDto ToDto(Airport airport) =>
            new(airport.IataCode, airport.Name, airport.City, airport.Country, airport.IsPopular);
    }
}
