using GoVoylo.Application.Features.Authentication.Commands.Login;
using GoVoylo.Application.Features.Authentication.Commands.LoginWithOtp;
using GoVoylo.Application.Features.Authentication.Commands.Logout;
using GoVoylo.Application.Features.Authentication.Commands.RefreshToken;
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
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
          LoginCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("login-otp")]
        public async Task<IActionResult> LoginWithOtp(LoginWithOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _mediator.Send(new LogoutCommand(request.RefreshToken));
            return Ok(new { message = "Logged out successfully." });
        }

        // Forgot password: request the OTP via POST /api/auth/send-otp, then confirm here.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

    public record RefreshTokenRequest(string RefreshToken);
}
