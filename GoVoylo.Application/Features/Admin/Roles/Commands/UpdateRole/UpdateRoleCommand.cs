using GoVoylo.Application.Features.Admin.Roles.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.UpdateRole
{
    public record UpdateRoleCommand(Guid RoleId, string Name) : IRequest<RoleDto>;
}
