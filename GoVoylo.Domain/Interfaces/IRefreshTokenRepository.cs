using GoVoylo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task SaveAsync(
            RefreshToken refreshToken);

        Task<RefreshToken?> GetByTokenHashAsync(
            string tokenHash);

        Task UpdateAsync(
            RefreshToken refreshToken);
    }
}
