using Microsoft.AspNetCore.Mvc;
using MediatR;
using GoVoylo.Application.Features.B2bIntegrations.Queries.TestTripJackSearch;

namespace GoVoylo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class B2bTestController : ControllerBase
{
    private readonly IMediator _mediator;

    public B2bTestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tripjack-search")]
    public async Task<IActionResult> TestSearch(
        [FromQuery] string origin,
        [FromQuery] string destination,
        [FromQuery] DateTime departureDate,
        CancellationToken ct)
    {
        var query = new TestTripJackSearchQuery(origin, destination, departureDate);
        var result = await _mediator.Send(query, ct);

        // Return Content directly as application/json so it preserves formatting in your browser/Postman
        return Content(result, "application/json");
    }
    [HttpGet("flights/search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchFlights(
       [FromQuery] string origin,
       [FromQuery] string destination,
       [FromQuery] DateTime departureDate,
       CancellationToken ct)
    {
        // 1. Basic Parameter Input Validation Guard
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            return BadRequest(new { message = "Origin and Destination codes are mandatory parameters." });
        }

        if (departureDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Departure date cannot be a past date." });
        }

        // 2. Wrap incoming parameters into our Application CQRS Query object
        var query = new TestTripJackSearchQuery(
            origin.ToUpper().Trim(),
            destination.ToUpper().Trim(),
            departureDate
        );

        // 3. Dispatch to our Clean Architecture pipeline via MediatR
        var rawJsonResult = await _mediator.Send(query, ct);

        // 4. Return Content explicitly formatted as application/json 
        // This ensures the frontend parses it directly as an object, not as a flat string literal.
        return Content(rawJsonResult, "application/json");
    }
}
