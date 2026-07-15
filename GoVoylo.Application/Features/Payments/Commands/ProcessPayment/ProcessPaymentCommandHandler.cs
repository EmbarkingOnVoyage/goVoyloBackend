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
    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        // 1. Instantiate domain entity executing encapsulated business invariants
        var transaction = PaymentTransaction.Create(
            request.Amount, 
            request.Currency, 
            request.SourceClient
        );
         // 2. Persist using the abstraction contract
        await _paymentRepository.SaveAsync(transaction, cancellationToken);

        // 3. Return mapped DTO output
        return new PaymentResponseDto(
            transaction.Id,
            transaction.ReferenceNumber,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt
        );
    }
}