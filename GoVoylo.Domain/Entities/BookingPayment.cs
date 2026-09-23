using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities;
public class BookingPayment : BaseEntity
{
    public string BookingReference { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string PaymentStatus { get; private set; } // e.g., Pending, Succeeded, Failed

    // Razorpay's own identifiers for this payment — Order_Id is assigned when
    // the order is created (before checkout opens) and is how the verify step
    // looks this record back up; Payment_Id only exists once checkout succeeds.
    public string? GatewayOrderId { get; private set; }
    public string? GatewayPaymentId { get; private set; }

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

    public void SetGatewayOrder(string gatewayOrderId) => GatewayOrderId = gatewayOrderId;

    public void MarkAsSucceeded(string? gatewayPaymentId = null)
    {
        PaymentStatus = "Succeeded";
        if (gatewayPaymentId != null) GatewayPaymentId = gatewayPaymentId;
    }

    public void MarkAsFailed() => PaymentStatus = "Failed";
}