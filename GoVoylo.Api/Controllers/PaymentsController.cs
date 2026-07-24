// GoVoylo.Api/Controllers/PaymentsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("HeavyTrafficPolicy")] // Protects all actions in this controller from traffic bursts
public class PaymentsController : ControllerBase
{
    private readonly ISender _mediator;

    // We inject ISender (MediatR interface) keeping our controller fully decoupled from business logic
    public PaymentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)] // Handle heavy rate limiting response
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        if (command == null)
            return BadRequest("Invalid payment payload.");

        // MediatR intercepts the command and safely sends it to your ProcessPaymentCommandHandler
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}
