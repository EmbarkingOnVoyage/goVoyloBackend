using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<Unit>;
}
