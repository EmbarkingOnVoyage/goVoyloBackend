using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.VerifyOtp
{
    public record VerifyOtpCommand(
       string Email,
       string VerificationToken,
       string Otp
   ) : IRequest<VerifyOtpResponseDto>;
}
