using GoVoylo.Application.Common;
using GoVoylo.Application.Features.Admin.Users.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Users.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PagedResult<AdminUserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public SearchUsersQueryHandler(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<PagedResult<AdminUserDto>> Handle(
            SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var (users, totalCount) = await _userRepository.SearchAsync(
                request.Search, request.Status, request.Page, request.PageSize);

            var rolesByUser = await _userRoleRepository.GetRoleNamesForUsersAsync(users.Select(u => u.Id));

            var items = users.Select(u => new AdminUserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Phone,
                u.Status,
                rolesByUser.TryGetValue(u.Id, out var roles) ? roles : Array.Empty<string>(),
                u.CreatedAt)).ToList();

            return new PagedResult<AdminUserDto>(items, totalCount, request.Page, request.PageSize);
        }
    }
}
