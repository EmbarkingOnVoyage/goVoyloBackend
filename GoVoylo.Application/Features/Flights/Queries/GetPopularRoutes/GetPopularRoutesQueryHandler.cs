using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetPopularRoutes
{
    public class GetPopularRoutesQueryHandler
        : IRequestHandler<GetPopularRoutesQuery, IReadOnlyList<RouteDto>>
    {
        private const int TopRouteCount = 20;

        private readonly ISearchLogRepository _searchLogRepository;

        public GetPopularRoutesQueryHandler(ISearchLogRepository searchLogRepository)
        {
            _searchLogRepository = searchLogRepository;
        }

        public async Task<IReadOnlyList<RouteDto>> Handle(
            GetPopularRoutesQuery request, CancellationToken cancellationToken)
        {
            var routes = await _searchLogRepository.GetPopularRoutesAsync(TopRouteCount);

            return routes
                .Select(x => new RouteDto(x.Origin, x.Destination, x.SearchCount))
                .ToList();
        }
    }
}
