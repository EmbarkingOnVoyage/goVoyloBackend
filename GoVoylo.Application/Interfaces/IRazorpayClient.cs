using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Interfaces
{
    public interface IRazorpayClient
    {
        // Razorpay's public key id — safe to hand to the mobile/web client so
        // it can open the checkout SDK, unlike the key secret.
        string KeyId { get; }

        Task<RazorpayOrderResult> CreateOrderAsync(
            decimal amount, string currency, string receipt, CancellationToken cancellationToken);

        // Verifies Razorpay's HMAC-SHA256 signature over "{orderId}|{paymentId}"
        // using the account's key secret — this is what proves a checkout
        // success callback actually came from Razorpay and wasn't spoofed.
        bool VerifySignature(string orderId, string paymentId, string signature);
    }
}
