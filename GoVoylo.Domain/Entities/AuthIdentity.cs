using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class AuthIdentity
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string Provider { get; private set; } = null!;

        public string ProviderSubject { get; private set; } = null!;

        public DateTime CreatedAt { get; private set; }

        // Navigation property
        public User User { get; private set; } = null!;
    }
}
