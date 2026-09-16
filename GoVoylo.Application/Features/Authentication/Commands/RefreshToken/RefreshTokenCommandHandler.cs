using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = GoVoylo.Domain.Entities.RefreshToken;

namespace GoVoylo.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserRoleRepository _userRoleRepository;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IUserRoleRepository userRoleRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<LoginResponseDto> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var tokenHash = _refreshTokenService.Hash(request.RefreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (existingToken == null || !existingToken.IsActive)
            {
                throw new UnauthorizedAppException(
                    "invalid_refresh_token", "Refresh token is invalid or expired.");
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);

            if (user == null || user.Status != "active")
            {
                throw new UnauthorizedAppException("invalid_refresh_token", "Refresh token is invalid or expired.");
            }

            // Rotate: the old token is single-use — revoking it here means a stolen,
            // already-used token can't be replayed by an attacker.
            existingToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(existingToken);

            // Re-fetch roles fresh (not copied from the old token) so a role change
            // takes effect on the very next refresh, not just on the next full login.
            var roles = await _userRoleRepository.GetRoleNamesForUserAsync(user.Id);
            var newAccessToken = _jwtTokenService.GenerateToken(user, roles);
            var newRawRefreshToken = _refreshTokenService.GenerateRawToken();
            var newRefreshToken = new RefreshTokenEntity(
                user.Id,
                _refreshTokenService.Hash(newRawRefreshToken),
                existingToken.DeviceInfo,
                _refreshTokenService.GetExpiryDate());

            await _refreshTokenRepository.SaveAsync(newRefreshToken);

            return new LoginResponseDto
            {
                Id = user.Id,
                Message = "Token refreshed successfully.",
                AccessToken = newAccessToken,
                RefreshToken = newRawRefreshToken
            };
        }
    }
}
