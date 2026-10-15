using GoVoylo.Application.Features.Pricing.Queries.GetConvenienceFeeOffer;
using GoVoylo.Application.Features.Pricing.Queries.GetConvenienceFeeRules;
using GoVoylo.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Route("api/v1/pricing")]
    public class PricingController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUser;

        public PricingController(ISender mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        // Public like flight search: guests see the fee before signing in to book.
        [HttpGet("convenience-fee-rules")]
        public async Task<IActionResult> GetConvenienceFeeRules(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetConvenienceFeeRulesQuery(), cancellationToken);
            return Ok(result);
        }

        // The signed-in customer's own fee offer (first booking free until a date).
        [Authorize]
        [HttpGet("convenience-fee-offer")]
        public async Task<IActionResult> GetConvenienceFeeOffer(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetConvenienceFeeOfferQuery(_currentUser.UserId), cancellationToken);
            return Ok(result);
        }
    }
}
