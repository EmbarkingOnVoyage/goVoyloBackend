using MediatR;

namespace GoVoylo.Application.Features.Admin.Users.Commands.UpdateCustomerStatus
{
    public record UpdateCustomerStatusCommand(
        Guid AdminUserId,
        Guid TargetUserId,
        string Status) : IRequest<Unit>;
}
