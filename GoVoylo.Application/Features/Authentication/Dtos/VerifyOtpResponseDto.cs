using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Dtos
{
    public class VerifyOtpResponseDto
    {
        public bool IsVerified { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
