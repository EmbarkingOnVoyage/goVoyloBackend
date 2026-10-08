using GoVoylo.Application.Features.Pricing.Queries.GetConvenienceFeeRules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Route("api/v1/pricing")]
    public class PricingController : ControllerBase
    {
        private readonly ISender _mediator;

        public PricingController(ISender mediator)
        {
            _mediator = mediator;
        }

        // Public like flight search: guests see the fee before signing in to book.
        [HttpGet("convenience-fee-rules")]
        public async Task<IActionResult> GetConvenienceFeeRules(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetConvenienceFeeRulesQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
