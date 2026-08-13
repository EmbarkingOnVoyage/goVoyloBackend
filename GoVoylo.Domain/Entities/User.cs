using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }

        public string? Email { get; private set; }

        public string? Phone { get; private set; }

        public string? PhoneCountryCode { get; private set; }

        public string? PasswordHash { get; private set; }

        public string FirstName { get; private set; } = null!;

        public string LastName { get; private set; } = null!;

        public bool IsEmailVerified { get; private set; }

        public bool IsPhoneVerified { get; private set; }

        public string Status { get; private set; } = "active";

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }


        public User(
        string email,
        string passwordHash,
        string? phone,
        string firstName,
        string lastName)
        {
            Id = Guid.NewGuid();

            Email = email;
            PasswordHash = passwordHash;
            Phone = phone;

            FirstName = firstName;
            LastName = lastName;

            IsEmailVerified = false;
            IsPhoneVerified = false;

            Status = "active";

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private User()
        {
        }

        public void ResetPassword(string passwordHash)
        {
            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
