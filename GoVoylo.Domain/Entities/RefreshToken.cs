using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string TokenHash { get; private set; } = null!;

        public string? DeviceInfo { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        // Navigation property
        public User User { get; private set; } = null!;
    }
}
