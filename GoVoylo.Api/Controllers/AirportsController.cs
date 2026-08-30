using System.Security.Claims;
using GoVoylo.Application.Features.Airports.Queries.GetAirport;
using GoVoylo.Application.Features.Airports.Queries.GetPopularAirports;
using GoVoylo.Application.Features.Airports.Queries.GetRecentAirportSearches;
using GoVoylo.Application.Features.Airports.Queries.SearchAirports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Route("api/v1/airports")]
    public class AirportsController : ControllerBase
    {
        private readonly ISender _mediator;

        public AirportsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var result = await _mediator.Send(new SearchAirportsQuery(q));
            return Ok(result);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var result = await _mediator.Send(new GetPopularAirportsQuery());
            return Ok(result);
        }

        [Authorize]
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetRecentAirportSearchesQuery(userId));
            return Ok(result);
        }

        [HttpGet("{iata}")]
        public async Task<IActionResult> GetByIata(string iata)
        {
            var result = await _mediator.Send(new GetAirportQuery(iata));
            return Ok(result);
        }
    }
}
