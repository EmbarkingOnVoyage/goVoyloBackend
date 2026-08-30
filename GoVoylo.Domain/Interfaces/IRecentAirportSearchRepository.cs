using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IRecentAirportSearchRepository
    {
        Task<RecentAirportSearch?> GetAsync(Guid userId, string iataCode);
        Task<IReadOnlyList<RecentAirportSearch>> GetRecentAsync(Guid userId, int limit);
        Task AddAsync(RecentAirportSearch search);
        Task UpdateAsync(RecentAirportSearch search);
    }
}
