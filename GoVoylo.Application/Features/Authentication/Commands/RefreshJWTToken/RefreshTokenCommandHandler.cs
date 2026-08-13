using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken
{
    public class RefreshTokenCommandHandler
        : IRequestHandler<
            RefreshTokenCommand,
            RefreshTokenResponseDto>
    {
        private readonly IRefreshTokenRepository
            _refreshTokenRepository;

        private readonly IUserRepository
            _userRepository;

        private readonly IJwtTokenService
            _jwtTokenService;

        private readonly IRefreshTokenService
            _refreshTokenService;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IConfiguration configuration)
        {
            _refreshTokenRepository =
                refreshTokenRepository;

            _userRepository =
                userRepository;

            _jwtTokenService =
                jwtTokenService;

            _refreshTokenService =
                refreshTokenService;

            _configuration = configuration;
        }

        public async Task<RefreshTokenResponseDto> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Hash the refresh token received from client
            var tokenHash =
                _refreshTokenService.HashToken(
                    request.TokenRefresh);

            // 2. Find refresh token in database
            var storedToken =
                await _refreshTokenRepository
                    .GetByTokenHashAsync(tokenHash);

            // 3. Token doesn't exist
            if (storedToken == null)
            {
                throw new Exception(
                    "Invalid refresh token.");
            }

            // 4. Check if token was revoked
            if (storedToken.IsRevoked())
            {
                throw new Exception(
                    "Refresh token has been revoked.");
            }

            // 5. Check expiry
            if (storedToken.IsExpired())
            {
                throw new Exception(
                    "Refresh token has expired.");
            }

            // 6. Find user
            var user =
                await _userRepository
                    .GetByIdAsync(storedToken.UserId);

            if (user == null)
            {
                throw new Exception(
                    "User not found.");
            }

            // 7. Check user status
            if (user.Status != "active")
            {
                throw new Exception(
                    "User account is not active.");
            }

            // 8. Generate new access token
            var newAccessToken =
                _jwtTokenService.GenerateToken(user);

            // 9. Generate new refresh token
            var newRefreshToken =
                _refreshTokenService
                    .GenerateRefreshToken();

            // Refresh token expiry
            var refreshTokenExpiryDays =
                _configuration.GetValue<int>(
                    "Jwt:RefreshTokenExpiryDays");

            // 10. Hash new refresh token
            var newRefreshTokenHash =
                _refreshTokenService
                    .HashToken(newRefreshToken);

            // 11. Revoke old refresh token
            storedToken.Revoke();

            await _refreshTokenRepository
                .UpdateAsync(storedToken);

            // 12. Create new refresh token record
            var newRefreshTokenEntity =
                new RefreshToken(
                    user.Id,
                    newRefreshTokenHash,
                    DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    storedToken.DeviceInfo);

            // 13. Save new refresh token
            await _refreshTokenRepository
                .SaveAsync(newRefreshTokenEntity);

            // 14. Return both tokens
            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
