using System.Security.Claims;
using GoVoylo.Application.Features.Flights.Commands.CancelBooking;
using GoVoylo.Application.Features.Flights.Commands.CreateBooking;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.GetFareCalendar;
using GoVoylo.Application.Features.Flights.Queries.GetFareRules;
using GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries;
using GoVoylo.Application.Features.Flights.Queries.GetSeatMap;
using GoVoylo.Application.Features.Flights.Queries.RepriceFlightOffer;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
using GoVoylo.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Route("api/v1/flights")]
    public class FlightsController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUser;

        public FlightsController(ISender mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] FlightSearchRequestDto request)
        {
            // Search itself needs no auth, but a caller who did attach a valid token
            // gets their searched airports tracked for recent-search recall.
            Guid? userId = null;
            if (User.Identity?.IsAuthenticated == true
                && Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var result = await _mediator.Send(new SearchFlightsQuery(request, userId));
            return Ok(result);
        }

        [HttpPost("offers/{offerId}/reprice")]
        public async Task<IActionResult> Reprice(Guid offerId)
        {
            var result = await _mediator.Send(new RepriceFlightOfferQuery(offerId));
            return Ok(result);
        }

        [HttpGet("fare-calendar")]
        public async Task<IActionResult> GetFareCalendar(
            [FromQuery] string origin, [FromQuery] string destination, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _mediator.Send(new GetFareCalendarQuery(origin, destination, month, year));
            return Ok(result);
        }

        [HttpGet("offers/{offerId}/ancillaries")]
        public async Task<IActionResult> GetAncillaries(Guid offerId)
        {
            var result = await _mediator.Send(new GetFlightAncillariesQuery(offerId));
            return Ok(result);
        }

        [HttpPost("offers/{offerId}/seatmap")]
        public async Task<IActionResult> GetSeatMap(
            Guid offerId, [FromBody] IReadOnlyList<SeatMapTravelerRequestDto> travelers)
        {
            var result = await _mediator.Send(new GetSeatMapQuery(offerId, travelers));
            return Ok(result);
        }

        [HttpPost("fare-rules")]
        public async Task<IActionResult> GetFareRules([FromBody] IReadOnlyList<Guid> offerIds)
        {
            var result = await _mediator.Send(new GetFareRulesQuery(offerIds));
            return Ok(result);
        }

        [Authorize]
        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            var command = new CreateBookingCommand(
                _currentUser.UserId, request.Legs, request.Travelers, request.PassengerMobile, request.PassengerEmail);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("bookings/cancel")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
