using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    public class GetFareRulesQueryHandler : IRequestHandler<GetFareRulesQuery, FareRulesResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetFareRulesQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<FareRulesResponseDto> Handle(
            GetFareRulesQuery request, CancellationToken cancellationToken)
        {
            var legs = new List<LegFareRulesDto>();

            foreach (var offerId in request.OfferIds)
            {
                var session = await _sessionStore.GetAsync(offerId, cancellationToken);

                if (session == null)
                {
                    throw new NotFoundException("Flight offer not found or has expired. Please search again.");
                }

                // Same reasoning as the other post-search Flyshop calls: reprice first
                // for a fresh Flight_Key/Fare_Id rather than trust the search-time one.
                var repriceResult = await _supplierClient.RepriceAsync(
                    new SupplierRepriceRequestDto(session.SearchKey, session.FlightKey, session.FareId),
                    cancellationToken);

                var updatedSession = session with
                {
                    FlightKey = repriceResult.FlightKey,
                    FareId = repriceResult.FareId
                };
                await _sessionStore.UpdateAsync(offerId, updatedSession, cancellationToken);

                var result = await _supplierClient.GetFareRulesAsync(
                    new SupplierFareRuleRequestDto(session.SearchKey, updatedSession.FlightKey, updatedSession.FareId),
                    cancellationToken);

                legs.Add(new LegFareRulesDto(
                    offerId,
                    result.Rules
                        .Select(r => new FareRuleDto(r.SegmentId, r.FareRuleName, r.FareRuleDesc))
                        .ToList()));
            }

            return new FareRulesResponseDto(legs);
        }
    }
}
