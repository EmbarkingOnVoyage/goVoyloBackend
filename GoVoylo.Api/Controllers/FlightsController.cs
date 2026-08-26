using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.RepriceFlightOffer;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
using MediatR;
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
            var result = await _mediator.Send(new SearchFlightsQuery(request));
            return Ok(result);
        }

        [HttpPost("offers/{offerId}/reprice")]
        public async Task<IActionResult> Reprice(Guid offerId)
        {
            var result = await _mediator.Send(new RepriceFlightOfferQuery(offerId));
            return Ok(result);
        }
    }
}
