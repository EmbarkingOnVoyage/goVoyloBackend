using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GoVoylo.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtpSettings = smtpOptions.Value;
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("GoVoylo", _smtpSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = "GoVoylo OTP Verification";

            message.Body = new TextPart("plain")
            {
                Text = $"Your OTP is: {otp}\n\nThis OTP is valid for 5 minutes."
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _smtpSettings.Host,
                _smtpSettings.Port,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _smtpSettings.Username,
                _smtpSettings.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
    }
}