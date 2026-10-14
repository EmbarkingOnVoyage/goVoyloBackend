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

        private AuthIdentity()
        {
        }

        // A sign-in method linked to an account: "google" / "apple" / "facebook"
        // with that provider's stable user id ("sub").
        public AuthIdentity(Guid userId, string provider, string providerSubject)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Provider = provider;
            ProviderSubject = providerSubject;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
