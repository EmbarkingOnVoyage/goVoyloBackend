using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.RevokeRole
{
    public record RevokeRoleCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;
}
