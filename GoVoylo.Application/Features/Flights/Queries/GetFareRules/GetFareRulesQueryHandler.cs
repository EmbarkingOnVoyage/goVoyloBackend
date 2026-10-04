using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    public class GetFareRulesQueryHandler : IRequestHandler<GetFareRulesQuery, FareRulesResponseDto>
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetFareRulesQueryHandler(
            IFlightSupplierClientResolver supplierClientResolver,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClientResolver = supplierClientResolver;
            _sessionStore = sessionStore;
        }

        public async Task<FareRulesResponseDto> Handle(
            GetFareRulesQuery request, CancellationToken cancellationToken)
        {
            var legs = new List<LegFareRulesDto>();

            if (request.OfferIds.Count == 0)
            {
                return new FareRulesResponseDto(legs);
            }

            // Same reasoning as the other post-search Flyshop calls: reprice first
            // for a fresh Flight_Key/Fare_Id rather than trust the search-time one.
            // The request's offers are the trip's legs, so they're repriced together
            // (see ItineraryReprice).
            var repriced = await ItineraryReprice.RepriceAsync(
                _sessionStore, _supplierClientResolver, request.OfferIds[0], request.OfferIds, cancellationToken);

            foreach (var leg in repriced)
            {
                var supplierClient = _supplierClientResolver.Resolve(leg.Session.SupplierCode);
                var result = await supplierClient.GetFareRulesAsync(
                    new SupplierFareRuleRequestDto(leg.Session.SearchKey, leg.Session.FlightKey, leg.Session.FareId),
                    cancellationToken);

                legs.Add(new LegFareRulesDto(
                    leg.OfferId,
                    result.Rules
                        .Select(r => new FareRuleDto(r.SegmentId, r.FareRuleName, r.FareRuleDesc))
                        .ToList()));
            }

            return new FareRulesResponseDto(legs);
        }
    }
}
