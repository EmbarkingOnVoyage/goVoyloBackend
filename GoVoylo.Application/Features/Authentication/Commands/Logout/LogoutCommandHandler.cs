using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, LogoutResponseDto>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;

        public LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<LogoutResponseDto> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            // Hash refresh token received from client
            var tokenHash =
                _refreshTokenService.HashToken(
                    request.RefreshToken);

            // Find token in database
            var storedToken =
                await _refreshTokenRepository
                    .GetByTokenHashAsync(tokenHash);

            // Token doesn't exist
            if (storedToken == null)
            {
                throw new Exception(
                    "Invalid refresh token.");
            }

            // Already revoked
            if (storedToken.IsRevoked())
            {
                return new LogoutResponseDto
                {
                    Message = "User already logged out."
                };
            }

            // Revoke token
            storedToken.Revoke();

            // Update database
            await _refreshTokenRepository
                .UpdateAsync(storedToken);

            return new LogoutResponseDto
            {
                Message = "Logout successful."
            };
        }
    }
}
