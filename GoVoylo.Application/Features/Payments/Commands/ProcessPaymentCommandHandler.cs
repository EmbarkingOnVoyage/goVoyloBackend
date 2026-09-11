//// The handler consumes Domain interfaces (IPaymentRepository) to execute 
//// business logic without knowing anything about PostgreSQL or MongoDB implementations.
//using MediatR;
//using GoVoylo.Domain.Entities;
//using GoVoylo.Domain.Interfaces;
//using GoVoylo.Application.Features.Payments.Dtos;

//namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment;

//public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
//{
//    private readonly IPaymentRepository _paymentRepository;
//    private readonly IActivityLogRepository _activityLogRepository;
//    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository, IActivityLogRepository activityLogRepository)
//    {
//        _paymentRepository = paymentRepository;
//        _activityLogRepository = activityLogRepository;
//    }

//    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
//    {
//        // 1. Instantiate domain entity executing encapsulated business invariants
//        var transaction = PaymentTransaction.Create(
//            request.Amount,
//            request.Currency,
//            request.SourceClient 
//        );
//        // 1. Enforce Domain Business Rules
//        var payment = new BookingPayment(request.BookingReference, request.Amount, request.Currency);

//        // 2. Persist using the abstraction contract
//        await _paymentRepository.SaveAsync(transaction, cancellationToken);
//        // 2. Persist tracking details (PostgreSQL target interface)
//        await _paymentRepository.SaveAsync(payment);

//       // 3. PREPARE THE ANALYTICAL ACTIVITY LOG FOR MONGODB
//        // (Instantiate your UserActivityLog entity with your required constructor arguments)
//        var logPayloadJson = $"{{\"Amount\":{request.Amount},\"Client\":\"{request.SourceClient}\"}}";

//        var activityLog = new UserActivityLog(
//            userId: Guid.NewGuid().ToString(), // Or grab current session user ID if available
//            actionType: "PaymentInitiated",
//            payloadJson: logPayloadJson,
//            sourcePlatform: request.SourceClient
//        );

//        // 4. DUMP TO MONGODB (Asynchronous NoSQL flat document streaming)
//        await _activityLogRepository.LogActivityAsync(activityLog, cancellationToken);

//        // 4. Map the domain entity state to your new DTO contract
//        return new PaymentResponseDto(
//            payment.Id,
//            payment.BookingReference,
//            payment.TotalAmount,
//            payment.Currency,
//            payment.PaymentStatus,
//            payment.CreatedAt 
//        );
//    }
//}

using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, CreatePaymentOrderResponseDto>
{

    private readonly IPaymentRepository _paymentRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository, IActivityLogRepository activityLogRepository)
    {
        _paymentRepository = paymentRepository;
        _activityLogRepository = activityLogRepository;
    }
    //private readonly IPaymentRepository _paymentRepository;
    //private readonly IActivityLogRepository _activityLogRepository;
    private readonly IRazorpayService _razorpayService;
    private readonly IConfiguration _configuration;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IActivityLogRepository activityLogRepository,
        IRazorpayService razorpayService,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _activityLogRepository = activityLogRepository;
        _razorpayService = razorpayService;
        _configuration = configuration;
    }

    public async Task<CreatePaymentOrderResponseDto> Handle(
        ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create your internal payment record
        var payment = new BookingPayment(
            request.BookingReference,
            request.Amount,
            request.Currency);

        // 2. Create Razorpay order
        var razorpayOrder = await _razorpayService.CreateOrderAsync(
            request.Amount,
            request.Currency,
            request.BookingReference,
            cancellationToken);

        // 3. Save Razorpay Order ID against your payment
        payment.SetPaymentProvider(razorpayOrder.OrderId);

        // 4. Save payment
        await _paymentRepository.SaveAsync(payment);

        // 5. Activity log
        var logPayloadJson =
            $"{{\"Amount\":{request.Amount},\"Client\":\"{request.SourceClient}\",\"RazorpayOrderId\":\"{razorpayOrder.OrderId}\"}}";

        var activityLog = new UserActivityLog(
            userId: Guid.NewGuid().ToString(),
            actionType: "PaymentInitiated",
            payloadJson: logPayloadJson,
            sourcePlatform: request.SourceClient
        );

        await _activityLogRepository.LogActivityAsync(
            activityLog,
            cancellationToken);

        // 6. Key ID is safe to send to frontend
        var razorpayKeyId =
            _configuration["Razorpay:KeyId"];

        return new CreatePaymentOrderResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency,
            razorpayKeyId!,
            razorpayOrder.OrderId
        );
    }
}