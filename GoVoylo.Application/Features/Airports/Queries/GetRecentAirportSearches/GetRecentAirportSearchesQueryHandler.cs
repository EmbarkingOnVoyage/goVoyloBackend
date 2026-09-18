using GoVoylo.Application.Features.Airports.Dtos;
using GoVoylo.Application.Features.Airports.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.GetRecentAirportSearches
{
    public class GetRecentAirportSearchesQueryHandler
        : IRequestHandler<GetRecentAirportSearchesQuery, IReadOnlyList<AirportDto>>
    {
        private const int MaxResults = 5;

        private readonly IRecentAirportSearchRepository _recentSearchRepository;
        private readonly IAirportRepository _airportRepository;

        public GetRecentAirportSearchesQueryHandler(
            IRecentAirportSearchRepository recentSearchRepository,
            IAirportRepository airportRepository)
        {
            _recentSearchRepository = recentSearchRepository;
            _airportRepository = airportRepository;
        }

        public async Task<IReadOnlyList<AirportDto>> Handle(
            GetRecentAirportSearchesQuery request, CancellationToken cancellationToken)
        {
            var recent = await _recentSearchRepository.GetRecentAsync(request.UserId, MaxResults);

            var results = new List<AirportDto>();
            foreach (var entry in recent)
            {
                var airport = await _airportRepository.GetByIataAsync(entry.IataCode);
                if (airport != null)
                {
                    results.Add(AirportMapper.ToDto(airport));
                }
            }

            return results;
        }
    }
}
