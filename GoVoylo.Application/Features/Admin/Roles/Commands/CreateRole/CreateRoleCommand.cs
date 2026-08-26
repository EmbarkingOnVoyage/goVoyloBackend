using GoVoylo.Application.Features.Admin.Roles.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.CreateRole
{
    public record CreateRoleCommand(string Name) : IRequest<RoleDto>;
}
