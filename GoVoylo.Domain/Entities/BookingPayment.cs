using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities;
public class BookingPayment : BaseEntity
{
    public string BookingReference { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string PaymentStatus { get; private set; } // e.g., Pending, Succeeded, Failed

    public BookingPayment(string bookingReference, decimal totalAmount, string currency)
    {
        if (string.IsNullOrWhiteSpace(bookingReference)) 
            throw new ArgumentException("Booking reference cannot be empty.");
        if (totalAmount <= 0) 
            throw new ArgumentException("Payment amount must be greater than zero.");

        BookingReference = bookingReference;
        TotalAmount = totalAmount;
        Currency = currency;
        PaymentStatus = "Pending";
    }

    public void MarkAsSucceeded() => PaymentStatus = "Succeeded";
    public void MarkAsFailed() => PaymentStatus = "Failed";
}