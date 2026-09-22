using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Interfaces;

public interface IPaymentProvider
{
    string ProviderName { get; }

    Task<PaymentOrderResult> CreateOrderAsync(
        PaymentOrderRequest request,
        CancellationToken cancellationToken);

    Task<PaymentVerificationResult> VerifyPaymentAsync(
        string orderId,
        string paymentId,
        string? signature,
        CancellationToken cancellationToken);

    public record PaymentVerificationResult(
        bool IsValid,
        string? PaymentMethod,
        string PaymentStatus
    );
}