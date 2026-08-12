using GoVoylo.Application.Features.Authentication.Commands.Login;
using GoVoylo.Application.Features.Authentication.Commands.Logout;
using GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken;
using GoVoylo.Application.Features.Authentication.Commands.Register;
using GoVoylo.Application.Features.Authentication.Commands.ResetPassword;
using GoVoylo.Application.Features.Authentication.Commands.SendOtp;
using GoVoylo.Application.Features.Authentication.Commands.VerifyOtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp(
            SendOtpCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(
    VerifyOtpCommand command)
        {
            var result =
                await _mediator.Send(command);

            return Ok(result);
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> User(
        //    RegisterUserCommand command)
        //{
        //    var result = await _mediator.Send(command);

        //    return Ok(result);
        //}

        [HttpPost("register")]
        public async Task<IActionResult> User(RegisterUserCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
          LoginCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(
    RefreshTokenCommand command)
        {
            var result =
                await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("Reset-password")]
        public async Task<IActionResult> ChangePassword(
         ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
    LogoutCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
