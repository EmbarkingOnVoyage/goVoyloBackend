using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordService _passwordService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IPasswordService passwordService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _passwordService = passwordService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponseDto> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Find user
            var user =
                await _userRepository.GetByEmailAsync(request.Email);

            // 2. User doesn't exist
            if (user == null)
            {
                throw new Exception("Invalid email or password.");
            }

            // 3. Check account status
            if (user.Status != "active")
            {
                throw new Exception("User account is not active.");
            }

            // 4. Check password
            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !_passwordService.VerifyPassword(
                    request.Password,
                    user.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }

            var accessToken =
                  _jwtTokenService.GenerateToken(user);

            var refreshToken =
                _refreshTokenService.GenerateRefreshToken();

            var refreshTokenHash =
                _refreshTokenService.HashToken(refreshToken);

            var refreshTokenEntity =
                new RefreshToken(
                    user.Id,
                    refreshTokenHash,
                    DateTime.UtcNow.AddDays(7));

            await _refreshTokenRepository
                .SaveAsync(refreshTokenEntity);

            // 5. Generate JWT
            var token =
                _jwtTokenService.GenerateToken(user);

            // 6. Return response
            return new LoginResponseDto
            {
                Id = user.Id,
                Message = "Login successful.",
                RefreshToken = refreshToken,
                AccessToken = token
            };
        }
    }
}
