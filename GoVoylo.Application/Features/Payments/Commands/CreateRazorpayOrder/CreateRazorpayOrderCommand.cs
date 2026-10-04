using MediatR;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.CreateRazorpayOrder
{
    // UserId is never bound from the request body — PaymentsController overwrites it
    // with the authenticated caller, so the handler can check booking ownership.
    public record CreateRazorpayOrderCommand(
        string BookingReference,
        decimal Amount,
        string Currency,
        string SourceClient,
        Guid UserId = default
    ) : IRequest<RazorpayOrderResponseDto>;
}
