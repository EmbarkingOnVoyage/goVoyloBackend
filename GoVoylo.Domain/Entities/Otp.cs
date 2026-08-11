using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class Otp
    {
        public Guid Id { get; private set; }

        public Guid? UserId { get; private set; }

        public string Destination { get; private set; } = null!;

        public string Purpose { get; private set; } = null!;

        public string OtpHash { get; private set; } = null!;

        public DateTime ExpiresAt { get; private set; }

        public DateTime? ConsumedAt { get; private set; }

        public short AttemptCount { get; private set; }

        public DateTime CreatedAt { get; private set; }

        // Navigation property
        public User? User { get; private set; }
    }
}
