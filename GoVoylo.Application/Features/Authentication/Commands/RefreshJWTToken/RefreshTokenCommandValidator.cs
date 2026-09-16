using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken
{
    public class RefreshTokenCommandValidator
        : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.TokenRefresh)
                .NotEmpty()
                .WithMessage("Refresh token is required.");
        }
    }
}
