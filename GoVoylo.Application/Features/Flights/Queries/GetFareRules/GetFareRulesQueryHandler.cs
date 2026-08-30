using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    // Tripjack's Air_FareRule returns a single free-text/HTML rule blob per segment that
    // covers cancellation AND change/reschedule terms together — it does not expose them
    // as separate structured endpoints, so this is the one "rules" call for both.
    public class GetFareRulesQueryHandler : IRequestHandler<GetFareRulesQuery, FareRulesResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetFareRulesQueryHandler(
            IFlightSupplierClient supplierClient, IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<FareRulesResponseDto> Handle(
            GetFareRulesQuery request, CancellationToken cancellationToken)
        {
            var session = await _sessionStore.GetAsync(request.OfferId, cancellationToken);

            if (session == null)
            {
                throw new NotFoundException("Flight offer not found or has expired. Please search again.");
            }

            var result = await _supplierClient.GetFareRulesAsync(
                session.SearchKey, session.FlightKey, session.FareId, cancellationToken);

            var rules = result.Rules
                .Select(r => new FareRuleDto(r.SegmentId, r.FareRuleName, r.FareRuleDescriptionHtml))
                .ToList();

            return new FareRulesResponseDto(request.OfferId, rules);
        }
    }
}
