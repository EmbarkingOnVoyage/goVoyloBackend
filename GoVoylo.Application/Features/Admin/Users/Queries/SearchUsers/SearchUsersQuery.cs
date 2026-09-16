using GoVoylo.Application.Common;
using GoVoylo.Application.Features.Admin.Users.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Users.Queries.SearchUsers
{
    public record SearchUsersQuery(
        string? Search,
        string? Status,
        int Page,
        int PageSize) : IRequest<PagedResult<AdminUserDto>>;
}
