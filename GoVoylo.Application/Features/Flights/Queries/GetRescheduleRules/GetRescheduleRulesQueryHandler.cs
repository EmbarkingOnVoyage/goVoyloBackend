using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetRescheduleRules
{
    // Tripjack does not expose a separate reschedule-rules endpoint — the reschedule
    // penalty schedule (RescheduleCharges) comes back as part of each fare in Air_Search
    // /Air_Reprice and is already carried on the cached offer, so this reads it back from
    // the search result cache instead of making another supplier call.
    public class GetRescheduleRulesQueryHandler
        : IRequestHandler<GetRescheduleRulesQuery, IReadOnlyList<RescheduleChargeDto>>
    {
        private readonly IFlightSearchResultCache _resultCache;

        public GetRescheduleRulesQueryHandler(IFlightSearchResultCache resultCache)
        {
            _resultCache = resultCache;
        }

        public async Task<IReadOnlyList<RescheduleChargeDto>> Handle(
            GetRescheduleRulesQuery request, CancellationToken cancellationToken)
        {
            var offers = await _resultCache.GetAsync(request.SearchId, cancellationToken);

            if (offers == null)
            {
                throw new NotFoundException("This search has expired. Please search again.");
            }

            var offer = offers.FirstOrDefault(o => o.OfferId == request.OfferId);

            if (offer == null)
            {
                throw new NotFoundException("Flight offer not found in this search result.");
            }

            return offer.RescheduleCharges;
        }
    }
}
