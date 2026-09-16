using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.DeleteRole
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public DeleteRoleCommandHandler(
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId);

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (RoleNames.BuiltIn.Contains(role.Name))
            {
                throw new BusinessRuleException(
                    "system_role_protected",
                    $"'{role.Name}' is a built-in role referenced directly in authorization code and cannot be deleted.");
            }

            var assignedCount = await _userRoleRepository.CountByRoleIdAsync(role.Id);

            if (assignedCount > 0)
            {
                throw new ConflictException(
                    "role_in_use",
                    $"This role is still assigned to {assignedCount} user(s) — revoke it from them first.");
            }

            await _roleRepository.DeleteAsync(role);
            return Unit.Value;
        }
    }
}
