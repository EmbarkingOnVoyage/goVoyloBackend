using GoVoylo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Interfaces
{
    public interface IOtpRepository
    {
        Task SaveAsync(OtpVerification otp);

        Task UpdateAsync(OtpVerification otp);
        //Task UpdateAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task <OtpVerification?> GetActiveOtpByEmailAsync(string email);

        Task<OtpVerification?> GetByTokenAsync(string token);
    }
}
