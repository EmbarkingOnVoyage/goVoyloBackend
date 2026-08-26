using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class TravelerSpecialAssistanceRepository : ITravelerSpecialAssistanceRepository
    {
        private readonly ApplicationDbContext _context;

        public TravelerSpecialAssistanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TravelerSpecialAssistance>> GetByTravelerIdAsync(Guid savedTravelerId)
        {
            return await _context.TravelerSpecialAssistances
                .Where(x => x.SavedTravelerId == savedTravelerId)
                .ToListAsync();
        }

        public async Task ReplaceAllAsync(Guid savedTravelerId, IEnumerable<TravelerSpecialAssistance> items)
        {
            var existing = await _context.TravelerSpecialAssistances
                .Where(x => x.SavedTravelerId == savedTravelerId)
                .ToListAsync();

            _context.TravelerSpecialAssistances.RemoveRange(existing);
            await _context.TravelerSpecialAssistances.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
    }
}
