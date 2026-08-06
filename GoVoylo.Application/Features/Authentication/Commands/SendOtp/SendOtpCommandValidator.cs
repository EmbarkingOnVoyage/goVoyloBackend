using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.SendOtp
{
    public class SendOtpCommandValidator: AbstractValidator <SendOtpCommand>
    {
        public SendOtpCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();       
        }
    }
}
