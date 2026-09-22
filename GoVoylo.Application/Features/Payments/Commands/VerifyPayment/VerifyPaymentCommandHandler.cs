using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyPayment;

public class VerifyPaymentCommandHandler
    : IRequestHandler<VerifyPaymentCommand, VerifyPaymentResponseDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProviderResolver _providerResolver;

    public VerifyPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentProviderResolver providerResolver)
    {
        _paymentRepository = paymentRepository;
        _providerResolver = providerResolver;
    }

    public async Task<VerifyPaymentResponseDto> Handle(
        VerifyPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get the payment record from our database
        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId,
            cancellationToken);

        if (payment == null)
        {
            throw new KeyNotFoundException(
                $"Payment with ID '{request.PaymentId}' was not found.");
        }

        // 2. Check that the provider matches the payment record
        if (!string.Equals(
                payment.PaymentProvider,
                request.PaymentProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Payment provider does not match the payment record.");
        }

        // 3. Check that the provider order ID matches
        if (!string.Equals(
                payment.ProviderOrderId,
                request.ProviderOrderId,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Provider order ID does not match the payment record.");
        }

        // 4. Get the correct payment provider
        var provider = _providerResolver.GetProvider(
            request.PaymentProvider);

        // 5. Verify payment with the selected provider
        //var isVerified = await provider.VerifyPaymentAsync(
        //    request.ProviderOrderId,
        //    request.ProviderPaymentId,
        //    request.ProviderSignature,
        //    cancellationToken);

        var verificationResult = await provider.VerifyPaymentAsync(
     request.ProviderOrderId,
     request.ProviderPaymentId,
     request.ProviderSignature,
     cancellationToken);

        // 6. If verification failed
        if (!verificationResult.IsValid)
        {
            payment.MarkAsFailed();

            await _paymentRepository.UpdateAsync(payment);

            throw new InvalidOperationException(
                "Payment verification failed.");
        }

        payment.SetPaymentMethod(
    verificationResult.PaymentMethod ?? "Unknown");

        payment.MarkAsSucceeded(
            request.ProviderPaymentId);

        await _paymentRepository.UpdateAsync(payment);

        // 9. Return payment result
        return new VerifyPaymentResponseDto(
            payment.Id,
            payment.PaymentStatus,
            payment.PaymentProvider!,
            payment.ProviderOrderId!,
            payment.ProviderPaymentId,
            payment.PaidAt);
    }
}