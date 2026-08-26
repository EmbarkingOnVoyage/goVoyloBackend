using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(Guid userId)
        {
            return await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> GetRoleNamesForUsersAsync(
            IEnumerable<Guid> userIds)
        {
            var rows = await _context.UserRoles
                .Where(x => userIds.Contains(x.UserId))
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .ToListAsync();

            return rows
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    IReadOnlyList<string> (g) => g.Select(x => x.Name).ToList());
        }

        public Task<bool> HasRoleAsync(Guid userId, Guid roleId)
        {
            return _context.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId);
        }

        public Task<int> CountByRoleIdAsync(Guid roleId)
        {
            return _context.UserRoles.CountAsync(x => x.RoleId == roleId);
        }

        public async Task AssignAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task<UserRole?> GetAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
        }

        public async Task RemoveAsync(UserRole userRole)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }
}
