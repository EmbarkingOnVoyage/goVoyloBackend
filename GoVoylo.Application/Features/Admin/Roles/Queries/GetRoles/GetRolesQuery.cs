using GoVoylo.Application.Features.Admin.Roles.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Queries.GetRoles
{
    public record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;
}
