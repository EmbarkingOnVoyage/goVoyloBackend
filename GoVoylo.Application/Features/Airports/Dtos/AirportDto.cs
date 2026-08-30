namespace GoVoylo.Application.Features.Airports.Dtos
{
    public record AirportDto(
        string IataCode,
        string Name,
        string City,
        string Country,
        bool IsPopular);
}
