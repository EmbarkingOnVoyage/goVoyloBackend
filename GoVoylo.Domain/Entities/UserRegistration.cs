using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public record UserRegistration
    {
        public Guid Id { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string Phone { get; private set; } = string.Empty;

        public string Password { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get;  private set; }

        public UserRegistration(
            string username,
            string email,
            string phone,
            string password)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            Phone = phone;
            Password = password;
            CreatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private UserRegistration()
        {
        }

    }
}
