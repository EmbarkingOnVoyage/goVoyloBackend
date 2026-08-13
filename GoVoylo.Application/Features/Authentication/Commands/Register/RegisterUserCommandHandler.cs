using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
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

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
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

            return new RegisterUserResponseDto
            {
                Id = user.Id,
                Message = "Registration successful."
            };
        }
    }
}