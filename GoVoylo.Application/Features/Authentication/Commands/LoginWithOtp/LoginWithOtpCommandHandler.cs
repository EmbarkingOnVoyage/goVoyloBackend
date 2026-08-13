using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = GoVoylo.Domain.Entities.RefreshToken;

namespace GoVoylo.Application.Features.Authentication.Commands.LoginWithOtp
{
    public class LoginWithOtpCommandHandler : IRequestHandler<LoginWithOtpCommand, LoginResponseDto>
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginWithOtpCommandHandler(
            IOtpRepository otpRepository,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponseDto> Handle(LoginWithOtpCommand request, CancellationToken cancellationToken)
        {
            // 1. Look up the OTP by verification token (from /api/auth/send-otp)
            var otpRecord = await _otpRepository.GetByTokenAsync(request.VerificationToken);

            if (otpRecord == null || !string.Equals(otpRecord.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAppException("invalid_otp", "Invalid verification token.");
            }

            if (DateTime.UtcNow > otpRecord.CreatedAt.AddMinutes(5))
            {
                throw new UnauthorizedAppException("otp_expired", "OTP has expired.");
            }

            if (otpRecord.Otp != request.Otp)
            {
                throw new UnauthorizedAppException("invalid_otp", "Invalid OTP.");
            }

            otpRecord.isVerified = true;
            await _otpRepository.UpdateAsync(otpRecord);

            // 2. OTP is valid — now find the account to log into
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new NotFoundException("No account found for this email.");
            }

            if (user.Status != "active")
            {
                throw new ForbiddenException("account_inactive", "User account is not active.");
            }

            // 3. Issue access + refresh tokens, same as password login
            var accessToken = _jwtTokenService.GenerateToken(user);

            var rawRefreshToken = _refreshTokenService.GenerateRawToken();
            var refreshToken = new RefreshTokenEntity(
                user.Id,
                _refreshTokenService.Hash(rawRefreshToken),
                deviceInfo: null,
                _refreshTokenService.GetExpiryDate());

            await _refreshTokenRepository.SaveAsync(refreshToken);

            return new LoginResponseDto
            {
                Id = user.Id,
                Message = "Login successful.",
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }
    }
}
