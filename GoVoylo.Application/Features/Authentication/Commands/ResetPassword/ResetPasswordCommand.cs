using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.ResetPassword
{
    public record ResetPasswordCommand(
    string Email,
    string OldPassword,
    string NewPassword
) : IRequest<ResetPasswordResponseDto>;
}
