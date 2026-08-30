using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetSearchHistory
{
    public class GetSearchHistoryQueryHandler
        : IRequestHandler<GetSearchHistoryQuery, IReadOnlyList<SearchHistoryDto>>
    {
        private const int MaxHistoryItems = 20;

        private readonly ISearchLogRepository _searchLogRepository;

        public GetSearchHistoryQueryHandler(ISearchLogRepository searchLogRepository)
        {
            _searchLogRepository = searchLogRepository;
        }

        public async Task<IReadOnlyList<SearchHistoryDto>> Handle(
            GetSearchHistoryQuery request, CancellationToken cancellationToken)
        {
            var logs = await _searchLogRepository.GetHistoryAsync(request.UserId, MaxHistoryItems);

            return logs
                .Select(x => new SearchHistoryDto(
                    x.Id, x.Origin, x.Destination, x.TravelDate, x.TripType, x.CabinClass, x.SearchedAt))
                .ToList();
        }
    }
}
