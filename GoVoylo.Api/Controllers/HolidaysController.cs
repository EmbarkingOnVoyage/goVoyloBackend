using GoVoylo.Application.Features.Holidays.Queries.GetHolidays;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Route("api/v1/holidays")]
    public class HolidaysController : ControllerBase
    {
        private readonly ISender _mediator;

        public HolidaysController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int year, [FromQuery] string country = "IN")
        {
            var result = await _mediator.Send(new GetHolidaysQuery(country, year));
            return Ok(result);
        }
    }
}
