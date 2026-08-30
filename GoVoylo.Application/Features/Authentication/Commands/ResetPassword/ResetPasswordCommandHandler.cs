using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponseDto>
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAuditService _auditService;

        public ResetPasswordCommandHandler(
            IOtpRepository otpRepository,
            IUserRepository userRepository,
            IPasswordService passwordService,
            IAuditService auditService)
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _passwordService = passwordService;
            _auditService = auditService;
        }

        public async Task<ResetPasswordResponseDto> Handle(
            ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify the OTP — same rules as LoginWithOtp: token must belong to this
            // email, must not have expired, and the code must match.
            var otpRecord = await _otpRepository.GetByTokenAsync(request.VerificationToken);

            if (otpRecord == null
                || !string.Equals(otpRecord.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAppException("invalid_otp", "Invalid verification token.");
            }

            if (otpRecord.isVerified)
            {
                throw new UnauthorizedAppException("otp_already_used", "This OTP has already been used.");
            }

            if (DateTime.UtcNow > otpRecord.CreatedAt.AddMinutes(5))
            {
                throw new UnauthorizedAppException("otp_expired", "OTP has expired.");
            }

            if (otpRecord.Otp != request.Otp)
            {
                throw new UnauthorizedAppException("invalid_otp", "Invalid OTP.");
            }

            // 2. OTP is valid — find the account. No old password required; that's the
            // point of a forgot-password flow.
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new NotFoundException("No account found for this email.");
            }

            if (user.Status != "active")
            {
                throw new ForbiddenException("account_inactive", "User account is not active.");
            }

            otpRecord.isVerified = true;
            await _otpRepository.UpdateAsync(otpRecord);

            user.ChangePasswordHash(_passwordService.HashPassword(request.NewPassword));
            await _userRepository.UpdateAsync(user);

            _auditService.Log(user.Id, AuditEventTypes.PasswordChanged);

            return new ResetPasswordResponseDto
            {
                Message = "Password reset successfully."
            };
        }
    }
}
