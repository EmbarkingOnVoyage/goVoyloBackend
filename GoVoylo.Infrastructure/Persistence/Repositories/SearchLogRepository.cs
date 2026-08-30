using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class SearchLogRepository : ISearchLogRepository
    {
        private readonly ApplicationDbContext _context;

        public SearchLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SearchLog log)
        {
            await _context.SearchLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<SearchLog>> GetHistoryAsync(Guid userId, int limit)
        {
            return await _context.SearchLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.SearchedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PopularRoute>> GetPopularRoutesAsync(int topN)
        {
            var grouped = await _context.SearchLogs
                .GroupBy(x => new { x.Origin, x.Destination })
                .Select(g => new { g.Key.Origin, g.Key.Destination, SearchCount = g.Count() })
                .OrderByDescending(x => x.SearchCount)
                .Take(topN)
                .ToListAsync();

            return grouped
                .Select(x => new PopularRoute(x.Origin, x.Destination, x.SearchCount))
                .ToList();
        }
    }
}
