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

        // Required by EF Core
        private RefreshToken()
        {
        }

        public RefreshToken(
            Guid userId,
            string tokenHash,
            DateTime expiresAt,
            string? deviceInfo = null)
        {
            Id = Guid.NewGuid();

            UserId = userId;
            TokenHash = tokenHash;
            DeviceInfo = deviceInfo;

            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;

            RevokedAt = null;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAt;
        }

        public bool IsRevoked()
        {
            return RevokedAt.HasValue;
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}
