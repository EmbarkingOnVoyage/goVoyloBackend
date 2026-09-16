using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; private set; }

        public Guid RoleId { get; private set; }

        public DateTime GrantedAt { get; private set; }

        public User User { get; private set; } = null!;

        public Role Role { get; private set; } = null!;

        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
            GrantedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private UserRole()
        {
        }
    }
}
