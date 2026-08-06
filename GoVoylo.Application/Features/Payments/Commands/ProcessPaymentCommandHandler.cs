// The handler consumes Domain interfaces (IPaymentRepository) to execute 
// business logic without knowing anything about PostgreSQL or MongoDB implementations.
using MediatR;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository, IActivityLogRepository activityLogRepository)
    {
        _paymentRepository = paymentRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        // 1. Instantiate domain entity executing encapsulated business invariants
        var transaction = PaymentTransaction.Create(
            request.Amount,
            request.Currency,
            request.SourceClient 
        );
        // 1. Enforce Domain Business Rules
        var payment = new BookingPayment(request.BookingReference, request.Amount, request.Currency);

        // 2. Persist using the abstraction contract
        await _paymentRepository.SaveAsync(transaction, cancellationToken);
        // 2. Persist tracking details (PostgreSQL target interface)
        await _paymentRepository.SaveAsync(payment);

        // 3. Dump analytics data for dropout tracking (MongoDB target interface)
        var log = new UserActivityLog(
            userId: "SYSTEM_USER", 
            actionType: "PaymentInitiated",
            payloadJson: $"{{ \"BookingReference\": \"{request.BookingReference}\" }}",
            sourcePlatform: request.SourceClient
        );
        await _activityLogRepository.LogActivityAsync(log);
       
        // 4. Map the domain entity state to your new DTO contract
        return new PaymentResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency,
            payment.PaymentStatus,
            payment.CreatedAt 
        );
    }
}