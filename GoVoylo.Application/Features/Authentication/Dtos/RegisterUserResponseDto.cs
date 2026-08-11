using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Dtos
{
    public record RegisterUserResponseDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }= string .Empty;
    }
}
