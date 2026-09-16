using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditService _auditService;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditService = auditService;
        }

        public async Task<RegisterUserResponseDto> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            // Check existing user
            var existingUser =
                await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new ConflictException("email_already_registered", "Email already registered.");
            }

            // Hash password
            var passwordHash =
                _passwordService.HashPassword(request.Password);

            // Create user
            var user = new User(
                request.Email,
                passwordHash,
                request.Phone,
                request.FirstName,
                request.LastName);

            // Save user
            await _userRepository.SaveAsync(user);

            // Every registered user starts as a plain customer
            var customerRole = await _roleRepository.GetByNameAsync("customer");

            if (customerRole != null)
            {
                await _userRoleRepository.AssignAsync(new UserRole(user.Id, customerRole.Id));
            }

            _auditService.Log(user.Id, AuditEventTypes.Registration);

            return new RegisterUserResponseDto
            {
                Id = user.Id,
                Message = "Registration successful."
            };
        }
    }
}