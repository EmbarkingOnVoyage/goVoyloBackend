using GoVoylo.Application.Features.Airports.Dtos;
using GoVoylo.Application.Features.Airports.Mappers;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.GetPopularAirports
{
    public class GetPopularAirportsQueryHandler
        : IRequestHandler<GetPopularAirportsQuery, IReadOnlyList<AirportDto>>
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IAirportCacheService _cache;

        public GetPopularAirportsQueryHandler(IAirportRepository airportRepository, IAirportCacheService cache)
        {
            _airportRepository = airportRepository;
            _cache = cache;
        }

        public async Task<IReadOnlyList<AirportDto>> Handle(
            GetPopularAirportsQuery request, CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync("airports:popular", async () =>
            {
                var airports = await _airportRepository.GetPopularAsync();
                return (IReadOnlyList<AirportDto>)airports.Select(AirportMapper.ToDto).ToList();
            });
        }
    }
}
