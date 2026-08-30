using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Filtering;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFilterSummary
{
    public class GetFilterSummaryQueryHandler : IRequestHandler<GetFilterSummaryQuery, FilterSummaryDto>
    {
        private readonly IFlightSearchResultCache _resultCache;

        public GetFilterSummaryQueryHandler(IFlightSearchResultCache resultCache)
        {
            _resultCache = resultCache;
        }

        public async Task<FilterSummaryDto> Handle(GetFilterSummaryQuery request, CancellationToken cancellationToken)
        {
            var offers = await _resultCache.GetAsync(request.SearchId, cancellationToken);

            if (offers == null)
            {
                throw new NotFoundException("This search has expired. Please search again.");
            }

            return FlightOfferFilterEngine.Summarize(offers);
        }
    }
}
