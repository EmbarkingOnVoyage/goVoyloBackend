using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Admin.Roles.Dtos;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.CreateRole
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
    {
        private readonly IRoleRepository _roleRepository;

        public CreateRoleCommandHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            if (await _roleRepository.ExistsByNameAsync(request.Name))
            {
                throw new ConflictException("role_already_exists", "A role with this name already exists.");
            }

            var role = new Role(request.Name);
            await _roleRepository.AddAsync(role);

            return new RoleDto(role.Id, role.Name);
        }
    }
}
