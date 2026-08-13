using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();

        string HashToken(string token);
    }
}
