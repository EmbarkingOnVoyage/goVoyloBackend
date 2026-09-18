using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class RecentAirportSearchRepository : IRecentAirportSearchRepository
    {
        private readonly ApplicationDbContext _context;

        public RecentAirportSearchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecentAirportSearch?> GetAsync(Guid userId, string iataCode)
        {
            var code = iataCode.ToUpperInvariant();
            return await _context.RecentAirportSearches
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IataCode == code);
        }

        public async Task<IReadOnlyList<RecentAirportSearch>> GetRecentAsync(Guid userId, int limit)
        {
            return await _context.RecentAirportSearches
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.SearchedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddAsync(RecentAirportSearch search)
        {
            await _context.RecentAirportSearches.AddAsync(search);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RecentAirportSearch search)
        {
            _context.RecentAirportSearches.Update(search);
            await _context.SaveChangesAsync();
        }
    }
}
