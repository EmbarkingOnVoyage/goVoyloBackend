using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponseDto>;
}
