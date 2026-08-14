using GoVoylo.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GoVoylo.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        public string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();
        }

        public string HashOtp(string otp)
        {
            return BCrypt.Net.BCrypt.HashPassword(otp);
        }

        public bool VerifyOtp(
            string otp,
            string otpHash)
        {
            return BCrypt.Net.BCrypt.Verify(
                otp,
                otpHash);
        }
    }
}
