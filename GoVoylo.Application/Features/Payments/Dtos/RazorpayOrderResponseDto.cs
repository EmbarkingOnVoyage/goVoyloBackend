namespace GoVoylo.Application.Features.Payments.Dtos
{
    // Sent back to the client so it can open Razorpay's checkout SDK — KeyId is
    // Razorpay's public identifier (safe to expose), never the key secret.
    public record RazorpayOrderResponseDto(
        string OrderId,
        long Amount,
        string Currency,
        string KeyId,
        string BookingReference);
}
