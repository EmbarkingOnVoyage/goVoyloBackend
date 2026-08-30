using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch
{
    public class SaveRecentAirportSearchCommandHandler : IRequestHandler<SaveRecentAirportSearchCommand, Unit>
    {
        private readonly IRecentAirportSearchRepository _recentSearchRepository;

        public SaveRecentAirportSearchCommandHandler(IRecentAirportSearchRepository recentSearchRepository)
        {
            _recentSearchRepository = recentSearchRepository;
        }

        public async Task<Unit> Handle(SaveRecentAirportSearchCommand request, CancellationToken cancellationToken)
        {
            var existing = await _recentSearchRepository.GetAsync(request.UserId, request.IataCode);

            if (existing != null)
            {
                existing.Touch();
                await _recentSearchRepository.UpdateAsync(existing);
            }
            else
            {
                await _recentSearchRepository.AddAsync(new RecentAirportSearch(request.UserId, request.IataCode));
            }

            return Unit.Value;
        }
    }
}
