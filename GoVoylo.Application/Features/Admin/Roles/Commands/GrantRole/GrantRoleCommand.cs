using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.GrantRole
{
    public record GrantRoleCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;
}
