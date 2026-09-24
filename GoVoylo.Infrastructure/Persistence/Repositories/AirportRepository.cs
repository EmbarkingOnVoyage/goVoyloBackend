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

        // The imported dataset (SeedData/airports.json) has no type/category field —
        // just a free-text Name — so military airbases (e.g. "Barksdale Air Force
        // Base", "Naval Air Station Nowra") can only be recognized by name pattern.
        // Genuinely dual-use fields that DO serve commercial traffic keep "Airport"
        // (or similar) in their name even when a military term is also present, e.g.
        // "Clark International Airport / Clark Air Base" or "Agra Airport / Agra Air
        // Force Station" — so a name is only treated as a pure airbase, and excluded,
        // when it matches a military keyword AND contains no civilian-airport term.
        private static readonly string[] MilitaryKeywords =
        {
            "air force base", "air force station", "afb", "air base", "airbase",
            "naval air station", "naval air facility", "marine corps air station",
            "military city"
        };

        private static readonly string[] CivilianTerms = { "airport", "air terminal" };

        private static bool IsPureAirbase(string name)
        {
            var lower = name.ToLowerInvariant();
            return MilitaryKeywords.Any(lower.Contains) && !CivilianTerms.Any(lower.Contains);
        }

        public async Task<IReadOnlyList<Airport>> SearchAsync(string query, int limit)
        {
            var term = query.ToLower();

            // Fetched in a generous buffer (not just `limit`) because the airbase
            // filter below runs in memory, after the DB query — filtering post-Take
            // could otherwise return fewer than `limit` real results for a term that
            // happens to match several airbases.
            var candidates = await _context.Airports
                .Where(a => a.IsActive && (
                    a.IataCode.ToLower() == term
                    || a.City.ToLower().Contains(term)
                    || a.Name.ToLower().Contains(term)))
                .OrderByDescending(a => a.IsPopular)
                .ThenBy(a => a.City)
                .Take(limit * 5)
                .ToListAsync();

            return candidates.Where(a => !IsPureAirbase(a.Name)).Take(limit).ToList();
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

        // Used by AirportImportService for the ~4,500-row master import — a
        // single AddRange + SaveChanges instead of one round trip per row.
        public async Task AddRangeAsync(IEnumerable<Airport> airports)
        {
            await _context.Airports.AddRangeAsync(airports);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Airport airport)
        {
            _context.Airports.Update(airport);
            await _context.SaveChangesAsync();
        }
    }
}
