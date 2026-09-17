using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Interfaces;

public interface IPaymentProvider
{
    string ProviderName { get; }

    Task<PaymentOrderResult> CreateOrderAsync(
        decimal amount,
        string currency,
        string bookingReference,
        CancellationToken cancellationToken);

    Task<PaymentVerificationResult> VerifyPaymentAsync(
        string orderId,
        string paymentId,
        string signature,
        CancellationToken cancellationToken);

    public record PaymentVerificationResult(
    bool IsVerified,
    string? PaymentMethod
);
}