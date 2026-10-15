using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries
{
    public class GetFlightAncillariesQueryHandler
        : IRequestHandler<GetFlightAncillariesQuery, FlightAncillariesResponseDto>
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetFlightAncillariesQueryHandler(
            IFlightSupplierClientResolver supplierClientResolver,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClientResolver = supplierClientResolver;
            _sessionStore = sessionStore;
        }

        public async Task<FlightAncillariesResponseDto> Handle(
            GetFlightAncillariesQuery request, CancellationToken cancellationToken)
        {
            // Air_GetSSR's own docs specify using the Flight_Key from an Air_Reprice
            // response, not the one from Air_Search — reprice here rather than assume
            // the search-time key is still valid, same as RepriceFlightOfferQueryHandler.
            var legs = await ItineraryReprice.RepriceAsync(
                _sessionStore, _supplierClientResolver, request.OfferId, request.ItineraryOfferIds, cancellationToken,
                request.FareIds);
            var leg = legs.Single(l => l.OfferId == request.OfferId);
            var supplierClient = _supplierClientResolver.Resolve(leg.Session.SupplierCode);

            var result = await supplierClient.GetAncillariesAsync(
                new SupplierAncillaryRequestDto(leg.Session.SearchKey, leg.Session.FlightKey),
                cancellationToken);

            // A combined review (one Tripjack bookingId for every leg) returns every
            // leg's options; this leg only wants its own.
            var combined = legs.Count > 1 && legs.All(l => l.Session.FlightKey == leg.Session.FlightKey);

            var options = result.Options
                .Where(o => !combined || o.LegIndex == leg.Position)
                .Select(o => new AncillaryOptionDto(
                    o.SsrType,
                    o.SsrTypeName,
                    o.SsrTypeDesc,
                    o.SsrCode,
                    o.SsrKey,
                    o.SsrStatus,
                    o.LegIndex,
                    o.SegmentId,
                    o.SegmentWise,
                    o.TotalAmount,
                    o.CurrencyCode,
                    o.ApplicablePaxTypes))
                .ToList();

            return new FlightAncillariesResponseDto(options);
        }
    }
}
