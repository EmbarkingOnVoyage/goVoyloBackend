using MediatR;
using GoVoylo.Domain.Interfaces; // Connects to your clean repository interface
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Queries;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDetailsDto?>
{
    private readonly IPaymentRepository _paymentRepository; // <-- Pure abstract interface

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDetailsDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        // Fetch data through the clean interface abstraction boundary
        var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (payment == null) return null;

        // Map your Domain Entity cleanly to your View Model/Query DTO
        return new PaymentDetailsDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency
        );
    }
}
