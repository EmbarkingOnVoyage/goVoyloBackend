using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities;
public class BookingPayment : BaseEntity
{
    public string BookingReference { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string PaymentStatus { get; private set; } // e.g., Pending, Succeeded, Failed

    public string? PaymentProvider { get; private set; }

    public string? ProviderOrderId { get; private set; }
    public string? ProviderPaymentId { get; private set; }
    public string? PaymentMethod { get; private set; }

    //public string? RazorpaySignature { get; private set; }

    public DateTime? PaidAt { get; private set; }
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

    public void SetPaymentProvider(string provider)
    {
        PaymentProvider = provider;
    }

    public void SetProviderOrderId(string orderId)
    {
        ProviderOrderId = orderId;
    }

    public void MarkAsSucceeded(
        string paymentId)
    //string signature
    {
        ProviderPaymentId = paymentId;
        //ProviderOrderId = paymentId;
        //RazorpaySignature = signature;
        PaymentStatus = "Succeeded";
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        PaymentStatus = "Failed";
    }

    public void SetPaymentMethod(string paymentMethod)
    {
        PaymentMethod = paymentMethod;
    }

    public void SetPaymentMethod(object value)
    {
        throw new NotImplementedException();
    }
    //public void MarkAsSucceeded() => PaymentStatus = "Succeeded";
    //public void MarkAsFailed() => PaymentStatus = "Failed";
}