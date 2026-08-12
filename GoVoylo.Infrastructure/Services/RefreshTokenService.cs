using GoVoylo.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace GoVoylo.Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        public string GenerateRefreshToken()
        {
            var randomBytes =
                RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }

        public string HashToken(string token)
        {
            var bytes =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }
    }
}