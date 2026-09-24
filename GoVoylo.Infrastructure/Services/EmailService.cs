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

        public async Task SendPassportExpiryAlertAsync(
            string email, string recipientName, string maskedPassportNumber, DateTime expiryDate)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("GoVoylo", _smtpSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = "Your passport is expiring soon";

            message.Body = new TextPart("plain")
            {
                Text =
                    $"Hi {recipientName},\n\n" +
                    $"Your passport ({maskedPassportNumber}) expires on {expiryDate:dd MMM yyyy}. " +
                    "Please renew it soon to avoid any disruption to your upcoming travel plans.\n\n" +
                    "— GoVoylo"
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

        // Sent after a Block_Ticket hold succeeds (see
        // CreateBookingCommandHandler / IFlightSupplierClient.CreateBlockTicketAsync)
        // — wording says "held", not "confirmed"/"booked", since Block_Ticket is a
        // reversible hold, not a final purchase.
        public async Task SendBookingConfirmationAsync(
            string email, string recipientName, string bookingRefNo, string? airlinePnr, string? recordLocator)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("GoVoylo", _smtpSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = $"Your GoVoylo booking reference: {bookingRefNo}";

            var bodyText = $"Hi {recipientName},\n\n" +
                $"Your flight has been held. Booking reference: {bookingRefNo}\n";

            if (!string.IsNullOrWhiteSpace(airlinePnr))
            {
                bodyText += $"Airline PNR: {airlinePnr}\n";
            }

            if (!string.IsNullOrWhiteSpace(recordLocator))
            {
                bodyText += $"Record locator: {recordLocator}\n";
            }

            bodyText += "\n— GoVoylo";

            message.Body = new TextPart("plain")
            {
                Text = bodyText
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