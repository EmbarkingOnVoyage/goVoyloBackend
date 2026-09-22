using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities;

public class BookingPayment : BaseEntity
{
    public string BookingReference { get; private set; }

    // Amount required for Flyshop/supplier
    public decimal SupplierAmount { get; private set; }

    // Commission received from Flyshop
    public decimal CommissionAmount { get; private set; }

    // Total amount customer pays
    public decimal TotalAmount { get; private set; }

    public string Currency { get; private set; }
    public string PaymentStatus { get; private set; }

    public string? PaymentProvider { get; private set; }
    public string? ProviderOrderId { get; private set; }
    public string? ProviderPaymentId { get; private set; }
    public string? PaymentMethod { get; private set; }

    public DateTime? PaidAt { get; private set; }

    public BookingPayment(
        string bookingReference,
        decimal supplierAmount,
        decimal commissionAmount,
        string currency)
    {
        if (string.IsNullOrWhiteSpace(bookingReference))
            throw new ArgumentException(
                "Booking reference cannot be empty.");

        if (supplierAmount <= 0)
            throw new ArgumentException(
                "Supplier amount must be greater than zero.");

        if (commissionAmount < 0)
            throw new ArgumentException(
                "Commission cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException(
                "Currency cannot be empty.");

        BookingReference = bookingReference;
        SupplierAmount = supplierAmount;
        CommissionAmount = commissionAmount;

        // Customer pays supplier amount + commission
        TotalAmount = supplierAmount + commissionAmount;

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

    public void MarkAsSucceeded(string paymentId)
    {
        ProviderPaymentId = paymentId;
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
}