using GoVoylo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task SaveAsync(User user);
    }
}
