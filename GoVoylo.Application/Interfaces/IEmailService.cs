using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IEmailService
    {
        public Task SendOtpAsync(string email, string otp);
    }
}
