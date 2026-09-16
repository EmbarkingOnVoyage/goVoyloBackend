using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class TravelerFrequentFlyerRepository : ITravelerFrequentFlyerRepository
    {
        private readonly ApplicationDbContext _context;

        public TravelerFrequentFlyerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelerFrequentFlyer?> GetByIdAsync(Guid id)
        {
            return await _context.TravelerFrequentFlyers.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<TravelerFrequentFlyer>> GetByTravelerIdAsync(Guid savedTravelerId)
        {
            return await _context.TravelerFrequentFlyers
                .Where(x => x.SavedTravelerId == savedTravelerId)
                .OrderBy(x => x.AirlineCode)
                .ToListAsync();
        }

        public Task<bool> ExistsForAirlineAsync(Guid savedTravelerId, string airlineCode)
        {
            return _context.TravelerFrequentFlyers
                .AnyAsync(x => x.SavedTravelerId == savedTravelerId && x.AirlineCode == airlineCode);
        }

        public async Task AddAsync(TravelerFrequentFlyer frequentFlyer)
        {
            await _context.TravelerFrequentFlyers.AddAsync(frequentFlyer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TravelerFrequentFlyer frequentFlyer)
        {
            _context.TravelerFrequentFlyers.Remove(frequentFlyer);
            await _context.SaveChangesAsync();
        }
    }
}
