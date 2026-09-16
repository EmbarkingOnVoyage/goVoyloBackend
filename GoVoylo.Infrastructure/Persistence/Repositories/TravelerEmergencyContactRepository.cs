using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class TravelerEmergencyContactRepository : ITravelerEmergencyContactRepository
    {
        private readonly ApplicationDbContext _context;

        public TravelerEmergencyContactRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelerEmergencyContact?> GetByIdAsync(Guid id)
        {
            return await _context.TravelerEmergencyContacts.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<TravelerEmergencyContact>> GetByTravelerIdAsync(Guid savedTravelerId)
        {
            return await _context.TravelerEmergencyContacts
                .Where(x => x.SavedTravelerId == savedTravelerId)
                .ToListAsync();
        }

        public async Task AddAsync(TravelerEmergencyContact contact)
        {
            await _context.TravelerEmergencyContacts.AddAsync(contact);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TravelerEmergencyContact contact)
        {
            _context.TravelerEmergencyContacts.Update(contact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TravelerEmergencyContact contact)
        {
            _context.TravelerEmergencyContacts.Remove(contact);
            await _context.SaveChangesAsync();
        }
    }
}
