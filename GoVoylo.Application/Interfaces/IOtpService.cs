using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();

        string HashOtp(string otp);

        bool VerifyOtp(string otp, string otpHash);
    }
}
