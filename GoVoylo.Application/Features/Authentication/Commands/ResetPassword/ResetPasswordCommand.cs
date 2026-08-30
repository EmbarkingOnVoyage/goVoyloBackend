using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.ResetPassword
{
    // Forgot-password flow: request an OTP via the existing POST /api/auth/send-otp,
    // then confirm here with that OTP — no old password required, since the whole
    // point is recovering an account whose password the customer no longer remembers.
    public record ResetPasswordCommand(
        string Email,
        string VerificationToken,
        string Otp,
        string NewPassword) : IRequest<ResetPasswordResponseDto>;
}
