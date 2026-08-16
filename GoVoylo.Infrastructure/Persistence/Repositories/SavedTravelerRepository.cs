using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class SavedTravelerRepository : ISavedTravelerRepository
    {
        private readonly ApplicationDbContext _context;

        public SavedTravelerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavedTraveler?> GetByIdAsync(Guid id)
        {
            return await _context.SavedTravelers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IReadOnlyList<SavedTraveler>> GetByUserIdAsync(Guid userId)
        {
            return await _context.SavedTravelers
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToListAsync();
        }

        public Task<int> CountByUserIdAsync(Guid userId)
        {
            return _context.SavedTravelers.CountAsync(x => x.UserId == userId && !x.IsDeleted);
        }

        public Task<bool> ExistsByIdentityAsync(
            Guid userId, string firstName, string lastName, DateTime dateOfBirth)
        {
            return _context.SavedTravelers.AnyAsync(x =>
                x.UserId == userId
                && !x.IsDeleted
                && x.DateOfBirth == dateOfBirth
                && x.FirstName.ToLower() == firstName.ToLower()
                && x.LastName.ToLower() == lastName.ToLower());
        }

        public async Task AddAsync(SavedTraveler traveler)
        {
            await _context.SavedTravelers.AddAsync(traveler);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SavedTraveler traveler)
        {
            _context.SavedTravelers.Update(traveler);
            await _context.SaveChangesAsync();
        }
    }
}
