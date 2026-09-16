using GoVoylo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task SaveAsync(User user);
        Task UpdateAsync(User user);
        Task<(IReadOnlyList<User> Users, int TotalCount)> SearchAsync(
            string? search, string? status, int page, int pageSize);
        Task<IReadOnlyList<User>> GetWithExpiringPassportUnnotifiedAsync(DateTime windowEnd);
    }
}
