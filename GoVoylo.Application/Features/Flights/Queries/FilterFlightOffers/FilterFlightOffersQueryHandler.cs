using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Filtering;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.FilterFlightOffers
{
    public class FilterFlightOffersQueryHandler
        : IRequestHandler<FilterFlightOffersQuery, FlightSearchResponseDto>
    {
        private readonly IFlightSearchResultCache _resultCache;

        public FilterFlightOffersQueryHandler(IFlightSearchResultCache resultCache)
        {
            _resultCache = resultCache;
        }

        public async Task<FlightSearchResponseDto> Handle(
            FilterFlightOffersQuery request, CancellationToken cancellationToken)
        {
            var offers = await _resultCache.GetAsync(request.Filter.SearchId, cancellationToken);

            if (offers == null)
            {
                throw new NotFoundException("This search has expired. Please search again.");
            }

            var filtered = FlightOfferFilterEngine.Apply(offers, request.Filter);

            return new FlightSearchResponseDto(request.Filter.SearchId, filtered);
        }
    }
}
