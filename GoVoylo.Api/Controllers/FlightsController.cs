using System.Security.Claims;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.FilterFlightOffers;
using GoVoylo.Application.Features.Flights.Queries.GetFareRules;
using GoVoylo.Application.Features.Flights.Queries.GetFilterSummary;
using GoVoylo.Application.Features.Flights.Queries.GetPopularRoutes;
using GoVoylo.Application.Features.Flights.Queries.GetRescheduleRules;
using GoVoylo.Application.Features.Flights.Queries.GetSearchHistory;
using GoVoylo.Application.Features.Flights.Queries.RepriceFlightOffer;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
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

        public FlightsController(ISender mediator)
        {
            _mediator = mediator;
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

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] FlightOfferFilterRequestDto request)
        {
            var result = await _mediator.Send(new FilterFlightOffersQuery(request));
            return Ok(result);
        }

        [HttpGet("filter-summary")]
        public async Task<IActionResult> GetFilterSummary([FromQuery] Guid searchId)
        {
            var result = await _mediator.Send(new GetFilterSummaryQuery(searchId));
            return Ok(result);
        }

        [Authorize]
        [HttpGet("search/history")]
        public async Task<IActionResult> GetSearchHistory()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetSearchHistoryQuery(userId));
            return Ok(result);
        }

        [HttpGet("popular-routes")]
        public async Task<IActionResult> GetPopularRoutes()
        {
            var result = await _mediator.Send(new GetPopularRoutesQuery());
            return Ok(result);
        }

        [HttpGet("offers/{offerId}/fare-rules")]
        public async Task<IActionResult> GetFareRules(Guid offerId)
        {
            var result = await _mediator.Send(new GetFareRulesQuery(offerId));
            return Ok(result);
        }

        [HttpGet("offers/{offerId}/reschedule-rules")]
        public async Task<IActionResult> GetRescheduleRules(Guid offerId, [FromQuery] Guid searchId)
        {
            var result = await _mediator.Send(new GetRescheduleRulesQuery(searchId, offerId));
            return Ok(result);
        }
    }
}
