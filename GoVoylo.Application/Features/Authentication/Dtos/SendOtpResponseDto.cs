using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Dtos
{
    public class SendOtpResponseDto
    {
        public string VerificationToken { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
