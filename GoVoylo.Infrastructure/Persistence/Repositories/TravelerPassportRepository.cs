using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class TravelerPassportRepository : ITravelerPassportRepository
    {
        private readonly ApplicationDbContext _context;

        public TravelerPassportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelerPassport?> GetByTravelerIdAsync(Guid savedTravelerId)
        {
            return await _context.TravelerPassports
                .FirstOrDefaultAsync(x => x.SavedTravelerId == savedTravelerId);
        }

        public async Task AddAsync(TravelerPassport passport)
        {
            await _context.TravelerPassports.AddAsync(passport);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TravelerPassport passport)
        {
            _context.TravelerPassports.Update(passport);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TravelerPassport passport)
        {
            _context.TravelerPassports.Remove(passport);
            await _context.SaveChangesAsync();
        }
    }
}
