using GoVoylo.Application.Features.Airports.Dtos;
using GoVoylo.Application.Features.Airports.Mappers;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.SearchAirports
{
    public class SearchAirportsQueryHandler : IRequestHandler<SearchAirportsQuery, IReadOnlyList<AirportDto>>
    {
        private const int MaxResults = 10;

        private readonly IAirportRepository _airportRepository;
        private readonly IAirportCacheService _cache;

        public SearchAirportsQueryHandler(IAirportRepository airportRepository, IAirportCacheService cache)
        {
            _airportRepository = airportRepository;
            _cache = cache;
        }

        public async Task<IReadOnlyList<AirportDto>> Handle(
            SearchAirportsQuery request, CancellationToken cancellationToken)
        {
            var term = request.Query.Trim().ToLowerInvariant();

            return await _cache.GetOrCreateAsync($"airports:search:{term}", async () =>
            {
                var airports = await _airportRepository.SearchAsync(term, MaxResults);
                return (IReadOnlyList<AirportDto>)airports.Select(AirportMapper.ToDto).ToList();
            });
        }
    }
}
