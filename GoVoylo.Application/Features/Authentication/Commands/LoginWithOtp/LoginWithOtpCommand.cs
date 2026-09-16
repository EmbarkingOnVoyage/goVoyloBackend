using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.LoginWithOtp
{
    public record LoginWithOtpCommand(
        string Email,
        string VerificationToken,
        string Otp) : IRequest<LoginResponseDto>;
}
