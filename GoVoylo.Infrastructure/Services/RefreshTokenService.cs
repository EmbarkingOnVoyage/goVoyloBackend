using System.Security.Cryptography;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;

        public RefreshTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DateTime GetExpiryDate()
        {
            var days = _configuration.GetValue<int?>("Jwt:RefreshTokenExpiryDays") ?? 30;
            return DateTime.UtcNow.AddDays(days);
        }

        public string GenerateRawToken()
        {
            // High-entropy opaque token — not user-chosen, so a fast hash (below) is
            // appropriate here, unlike passwords which need slow hashing (BCrypt).
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public string Hash(string rawToken)
        {
            var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(hashBytes);
        }
    }
}
