using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.SendOtp
{
    public record SendOtpCommand(string Email) : IRequest<SendOtpResponseDto>;

}
