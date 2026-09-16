using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken
{
    public record RefreshTokenCommand(
        string TokenRefresh
    ) : IRequest<RefreshTokenResponseDto>;

}
