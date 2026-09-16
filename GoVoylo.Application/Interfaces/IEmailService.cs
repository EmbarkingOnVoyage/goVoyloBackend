using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IEmailService
    {
        public Task SendOtpAsync(string email, string otp);

        public Task SendPassportExpiryAlertAsync(
            string email, string recipientName, string maskedPassportNumber, DateTime expiryDate);
    }
}
