using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler
    : IRequestHandler<ProcessPaymentCommand, CreatePaymentOrderResponseDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IPaymentProviderResolver _providerResolver;
    private readonly IConfiguration _configuration;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IActivityLogRepository activityLogRepository,
        IPaymentProviderResolver providerResolver,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _activityLogRepository = activityLogRepository;
        _providerResolver = providerResolver;
        _configuration = configuration;
    }

    public async Task<CreatePaymentOrderResponseDto> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Create internal payment record
        var payment = new BookingPayment(
            request.BookingReference,
            request.Amount,
            request.Currency);

        // 2. Get the selected payment provider
        var provider = _providerResolver.GetProvider("Razorpay");

        // 3. Create provider order
        var providerOrder = await provider.CreateOrderAsync(
            request.Amount,
            request.Currency,
            request.BookingReference,
            cancellationToken);

        // 4. Store provider details
        payment.SetPaymentProvider(provider.ProviderName);
        payment.SetProviderOrderId(providerOrder.OrderId);

        // 5. Save payment
        await _paymentRepository.SaveAsync(payment);

        // 6. Activity log
        var logPayloadJson =
            $"{{\"Amount\":{request.Amount},\"Client\":\"{request.SourceClient}\",\"Provider\":\"{provider.ProviderName}\",\"ProviderOrderId\":\"{providerOrder.OrderId}\"}}";

        var activityLog = new UserActivityLog(
            userId: Guid.NewGuid().ToString(),
            actionType: "PaymentInitiated",
            payloadJson: logPayloadJson,
            sourcePlatform: request.SourceClient
        );

        await _activityLogRepository.LogActivityAsync(
            activityLog,
            cancellationToken);

        // 7. Get Razorpay Key ID for frontend
        var razorpayKeyId =
            _configuration["Razorpay:KeyId"];

        // 8. Return order details
        return new CreatePaymentOrderResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency,
            razorpayKeyId!,
            providerOrder.OrderId
        );
    }
}