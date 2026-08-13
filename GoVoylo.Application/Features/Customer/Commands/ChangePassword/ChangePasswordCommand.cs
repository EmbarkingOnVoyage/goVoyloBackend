using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.ChangePassword
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword) : IRequest<Unit>;
}
