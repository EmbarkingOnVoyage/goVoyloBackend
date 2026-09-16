using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Admin.Roles.Dtos;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
    {
        private readonly IRoleRepository _roleRepository;

        public UpdateRoleCommandHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
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
                    $"'{role.Name}' is a built-in role referenced directly in authorization code and cannot be renamed.");
            }

            if (await _roleRepository.ExistsByNameAsync(request.Name))
            {
                throw new ConflictException("role_already_exists", "A role with this name already exists.");
            }

            role.Rename(request.Name);
            await _roleRepository.UpdateAsync(role);

            return new RoleDto(role.Id, role.Name);
        }
    }
}
