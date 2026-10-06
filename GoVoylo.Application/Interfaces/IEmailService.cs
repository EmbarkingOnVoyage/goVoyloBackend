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

        public Task SendBookingConfirmationAsync(
            string email, string recipientName, string bookingRefNo, string? airlinePnr, string? recordLocator);

        // The ticketed booking's e-ticket, attached as a PDF. routeSummary: "DEL–BOM, BOM–DEL".
        public Task SendETicketAsync(
            string email, string recipientName, string bookingRefNo, string routeSummary, byte[] pdf, string fileName);
    }
}
