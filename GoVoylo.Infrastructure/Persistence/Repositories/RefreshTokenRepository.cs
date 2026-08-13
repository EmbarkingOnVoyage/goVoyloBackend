using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository
        : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(
            RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(
                refreshToken);

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(
            string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash);
        }

        public async Task UpdateAsync(
            RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(
                refreshToken);

            await _context.SaveChangesAsync();
        }
    }
}
