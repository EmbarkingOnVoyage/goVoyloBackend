using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.GrantRole
{
    public class GrantRoleCommandHandler : IRequestHandler<GrantRoleCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public GrantRoleCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<Unit> Handle(GrantRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var role = await _roleRepository.GetByIdAsync(request.RoleId);

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (await _userRoleRepository.HasRoleAsync(request.UserId, request.RoleId))
            {
                throw new ConflictException("role_already_assigned", "This user already has this role.");
            }

            await _userRoleRepository.AssignAsync(new UserRole(request.UserId, request.RoleId));
            return Unit.Value;
        }
    }
}
