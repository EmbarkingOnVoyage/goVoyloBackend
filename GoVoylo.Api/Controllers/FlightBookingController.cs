using GoVoylo.Application.Features.Booking.Commands;
using GoVoylo.Application.Features.Booking.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FlightBookingController : ControllerBase
{
    private readonly ISender _mediator;

    public FlightBookingController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> BookFlight(
     [FromBody] BookFlightCommand command)
     {
        if (command == null)
            return BadRequest("Invalid booking payload.");

        var result = await _mediator.Send(command);

        return Ok(result);
      }

    [HttpGet("{bookingReference}")]
    public async Task<IActionResult> GetBooking(
    string bookingReference)
    {
        var query = new GetBookFlightQuery(bookingReference);

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}