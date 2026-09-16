using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class PreferenceRepository : IUserPreferenceRepository, INotificationPreferenceRepository
    {
        private readonly ApplicationDbContext _context;

        public PreferenceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserPreference?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task UpsertAsync(UserPreference preference)
        {
            var exists = await _context.UserPreferences.AnyAsync(x => x.UserId == preference.UserId);

            if (exists)
            {
                _context.UserPreferences.Update(preference);
            }
            else
            {
                await _context.UserPreferences.AddAsync(preference);
            }

            await _context.SaveChangesAsync();
        }

        Task<NotificationPreference?> INotificationPreferenceRepository.GetByUserIdAsync(Guid userId)
        {
            return _context.NotificationPreferences.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task UpsertAsync(NotificationPreference preference)
        {
            var exists = await _context.NotificationPreferences.AnyAsync(x => x.UserId == preference.UserId);

            if (exists)
            {
                _context.NotificationPreferences.Update(preference);
            }
            else
            {
                await _context.NotificationPreferences.AddAsync(preference);
            }

            await _context.SaveChangesAsync();
        }
    }
}
