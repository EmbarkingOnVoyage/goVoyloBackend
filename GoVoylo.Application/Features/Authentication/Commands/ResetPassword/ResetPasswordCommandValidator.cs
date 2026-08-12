using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator() 
        {
            RuleFor(x => x.Email)
                  .NotEmpty()
                  .EmailAddress()
                  .WithMessage("Valid Email Is Required");

            RuleFor(x => x.OldPassword)
                .NotEmpty()
                .WithMessage("Old Password is required");

            RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]")
            .WithMessage(
                "Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage(
                "Password must contain at least one lowercase letter.")
            .Matches(@"\d")
            .WithMessage(
                "Password must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage(
                "Password must contain at least one special character.");

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.OldPassword)
                .WithMessage(
                    "New password must be different from old password.");
        }
    }
}
