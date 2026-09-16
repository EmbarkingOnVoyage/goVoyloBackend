using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class OtpRepository: IOtpRepository
    {
        private readonly ApplicationDbContext _context;
        public OtpRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(OtpVerification otp)
        {
            _context.OtpVerifications.Add(otp);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OtpVerification otp)
        {
            _context.OtpVerifications.Update(otp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var otp = await _context.OtpVerifications.FindAsync(id);

            if (otp != null)
            {
                _context.OtpVerifications.Remove(otp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<OtpVerification?> GetActiveOtpByEmailAsync(string email)
        {
            return await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    !x.isVerified);
        }

        public async Task<OtpVerification?> GetByTokenAsync(string token)
        {
            return await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.VerificationToken == token);
        }
    }
}
