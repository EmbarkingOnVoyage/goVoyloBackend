using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuditService _auditService;

        public LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            IAuditService auditService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var tokenHash = _refreshTokenService.Hash(request.RefreshToken);
            var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            // Logout is idempotent — an already-invalid token is not an error.
            if (token != null && token.IsActive)
            {
                token.Revoke();
                await _refreshTokenRepository.UpdateAsync(token);
                _auditService.Log(token.UserId, AuditEventTypes.Logout);
            }

            return Unit.Value;
        }
    }
}
