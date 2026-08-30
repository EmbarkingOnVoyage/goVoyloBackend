using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class AirportRepository : IAirportRepository
    {
        private readonly ApplicationDbContext _context;

        public AirportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Airport>> SearchAsync(string query, int limit)
        {
            var term = query.ToLower();

            return await _context.Airports
                .Where(a => a.IsActive && (
                    a.IataCode.ToLower() == term
                    || a.City.ToLower().Contains(term)
                    || a.Name.ToLower().Contains(term)))
                .OrderByDescending(a => a.IsPopular)
                .ThenBy(a => a.City)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Airport?> GetByIataAsync(string iataCode)
        {
            var code = iataCode.ToUpperInvariant();
            return await _context.Airports.FirstOrDefaultAsync(a => a.IataCode == code);
        }

        public async Task<IReadOnlyList<Airport>> GetPopularAsync()
        {
            return await _context.Airports
                .Where(a => a.IsActive && a.IsPopular)
                .OrderBy(a => a.City)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Airports.CountAsync();
        }

        public async Task AddAsync(Airport airport)
        {
            await _context.Airports.AddAsync(airport);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Airport airport)
        {
            _context.Airports.Update(airport);
            await _context.SaveChangesAsync();
        }
    }
}
