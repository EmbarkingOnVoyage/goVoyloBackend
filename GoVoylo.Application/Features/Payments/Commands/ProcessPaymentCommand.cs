// Commands represent an intent to change the state of the system. They are immutable data structures
using MediatR;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.ProcessPayment
{
    public record ProcessPaymentCommand(
         string BookingReference,
         decimal Amount,
         string Currency,
         string SourceClient,
         string PaymentMethodToken

    ) : IRequest<PaymentResponseDto>;
}