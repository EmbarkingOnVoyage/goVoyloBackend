using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.DeleteRole
{
    public record DeleteRoleCommand(Guid RoleId) : IRequest<Unit>;
}
