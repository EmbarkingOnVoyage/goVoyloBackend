using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        // Returns the raw token to hand to the client — only the hash is ever persisted.
        string GenerateRawToken();
        string Hash(string rawToken);
        DateTime GetExpiryDate();
        //RefreshToken GenerateRefreshToken();
    }
}
