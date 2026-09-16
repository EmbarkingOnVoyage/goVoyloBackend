using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task SaveAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(IReadOnlyList<User> Users, int TotalCount)> SearchAsync(
            string? search, string? status, int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(x =>
                    x.FirstName.ToLower().Contains(term)
                    || x.LastName.ToLower().Contains(term)
                    || (x.Email != null && x.Email.ToLower().Contains(term))
                    || (x.Phone != null && x.Phone.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<IReadOnlyList<User>> GetWithExpiringPassportUnnotifiedAsync(DateTime windowEnd)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Users
                .Where(x => x.PassportNumberEncrypted != null
                    && x.PassportExpiryAlertSentAt == null
                    && x.PassportExpiryDate > today
                    && x.PassportExpiryDate <= windowEnd)
                .ToListAsync();
        }
    }
}
