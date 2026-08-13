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

        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        public RefreshToken(Guid userId, string tokenHash, string? deviceInfo, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            DeviceInfo = deviceInfo;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private RefreshToken()
        {
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}
