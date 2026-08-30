using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public record PopularRoute(string Origin, string Destination, int SearchCount);

    public interface ISearchLogRepository
    {
        Task AddAsync(SearchLog log);
        Task<IReadOnlyList<SearchLog>> GetHistoryAsync(Guid userId, int limit);
        Task<IReadOnlyList<PopularRoute>> GetPopularRoutesAsync(int topN);
    }
}
