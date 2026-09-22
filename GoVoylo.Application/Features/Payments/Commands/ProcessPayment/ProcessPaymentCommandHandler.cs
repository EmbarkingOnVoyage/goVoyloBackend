using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler
    : IRequestHandler<ProcessPaymentCommand, CreatePaymentOrderResponseDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IPaymentProviderResolver _providerResolver;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IActivityLogRepository activityLogRepository,
        IPaymentProviderResolver providerResolver)
    {
        _paymentRepository = paymentRepository;
        _activityLogRepository = activityLogRepository;
        _providerResolver = providerResolver;
    }

    public async Task<CreatePaymentOrderResponseDto> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var supplierAmount = request.BaseFare;
        var commissionAmount = request.Commission;
        var totalAmount = supplierAmount + commissionAmount;

        var payment = new BookingPayment(
            request.BookingReference,
            supplierAmount,
            commissionAmount,
            request.Currency);

        // Get selected payment gateway
        var provider = _providerResolver.GetProvider(
            request.PaymentProvider);

        // Create generic payment request
        var orderRequest = new PaymentOrderRequest(
            request.BookingReference,
            totalAmount,
            supplierAmount,
            commissionAmount,
            request.Currency);

        // Gateway-specific implementation happens inside the provider
        var providerOrder = await provider.CreateOrderAsync(
            orderRequest,
            cancellationToken);

        payment.SetPaymentProvider(
            provider.ProviderName);

        payment.SetProviderOrderId(
            providerOrder.OrderId);

        await _paymentRepository.SaveAsync(payment);

        var logPayloadJson =
            $"{{" +
            $"\"SupplierAmount\":{payment.SupplierAmount}," +
            $"\"CommissionAmount\":{payment.CommissionAmount}," +
            $"\"TotalAmount\":{payment.TotalAmount}," +
            $"\"Client\":\"{request.SourceClient}\"," +
            $"\"Provider\":\"{provider.ProviderName}\"," +
            $"\"ProviderOrderId\":\"{providerOrder.OrderId}\"" +
            $"}}";

        var activityLog = new UserActivityLog(
            userId: Guid.NewGuid().ToString(),
            actionType: "PaymentInitiated",
            payloadJson: logPayloadJson,
            sourcePlatform: request.SourceClient
        );

        await _activityLogRepository.LogActivityAsync(
            activityLog,
            cancellationToken);

        return new CreatePaymentOrderResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.SupplierAmount,
            payment.CommissionAmount,
            payment.TotalAmount,
            payment.Currency,
            provider.ProviderName,
            providerOrder.OrderId,
            providerOrder.PublicKey,
            providerOrder.CheckoutToken
        );
    }
}