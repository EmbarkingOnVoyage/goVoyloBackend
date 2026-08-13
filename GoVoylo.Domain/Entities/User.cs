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

        public string? ProfileImageUrl { get; private set; }

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

        public void UpdateProfile(string firstName, string lastName, string? phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePasswordHash(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProfileImageUrl(string url)
        {
            ProfileImageUrl = url;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearProfileImageUrl()
        {
            ProfileImageUrl = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkDeleted()
        {
            Status = "deleted";
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
