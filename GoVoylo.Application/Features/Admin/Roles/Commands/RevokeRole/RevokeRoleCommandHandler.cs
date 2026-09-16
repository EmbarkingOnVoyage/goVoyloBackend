using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.RevokeRole
{
    public class RevokeRoleCommandHandler : IRequestHandler<RevokeRoleCommand, Unit>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public RevokeRoleCommandHandler(
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<Unit> Handle(RevokeRoleCommand request, CancellationToken cancellationToken)
        {
            var assignment = await _userRoleRepository.GetAsync(request.UserId, request.RoleId);

            if (assignment == null)
            {
                throw new NotFoundException("This user does not have this role.");
            }

            var role = await _roleRepository.GetByIdAsync(request.RoleId);

            if (role != null && role.Name == RoleNames.Superadmin)
            {
                var superadminCount = await _userRoleRepository.CountByRoleIdAsync(request.RoleId);

                if (superadminCount <= 1)
                {
                    throw new BusinessRuleException(
                        "last_superadmin",
                        "Cannot revoke superadmin from the last remaining superadmin — grant it to someone else first.");
                }
            }

            await _userRoleRepository.RemoveAsync(assignment);
            return Unit.Value;
        }
    }
}
