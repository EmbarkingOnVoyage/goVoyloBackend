using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class TravelerVisaRepository : ITravelerVisaRepository
    {
        private readonly ApplicationDbContext _context;

        public TravelerVisaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelerVisa?> GetByIdAsync(Guid id)
        {
            return await _context.TravelerVisas.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<TravelerVisa>> GetByTravelerIdAsync(Guid savedTravelerId)
        {
            return await _context.TravelerVisas
                .Where(x => x.SavedTravelerId == savedTravelerId)
                .OrderBy(x => x.Country)
                .ToListAsync();
        }

        public Task<bool> ExistsForCountryAsync(Guid savedTravelerId, string country)
        {
            return _context.TravelerVisas
                .AnyAsync(x => x.SavedTravelerId == savedTravelerId && x.Country == country);
        }

        public async Task AddAsync(TravelerVisa visa)
        {
            await _context.TravelerVisas.AddAsync(visa);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TravelerVisa visa)
        {
            _context.TravelerVisas.Update(visa);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TravelerVisa visa)
        {
            _context.TravelerVisas.Remove(visa);
            await _context.SaveChangesAsync();
        }
    }
}
