using MediatR;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Commands.CreateRazorpayOrder
{
    public record CreateRazorpayOrderCommand(
        string BookingReference,
        decimal Amount,
        string Currency,
        string SourceClient
    ) : IRequest<RazorpayOrderResponseDto>;
}
