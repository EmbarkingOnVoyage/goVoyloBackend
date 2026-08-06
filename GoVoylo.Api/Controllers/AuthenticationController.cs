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
    }
}
