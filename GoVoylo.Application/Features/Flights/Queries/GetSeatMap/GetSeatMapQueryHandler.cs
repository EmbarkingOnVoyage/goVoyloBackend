using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetSeatMap
{
    public class GetSeatMapQueryHandler : IRequestHandler<GetSeatMapQuery, SeatMapResponseDto>
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetSeatMapQueryHandler(
            IFlightSupplierClientResolver supplierClientResolver,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClientResolver = supplierClientResolver;
            _sessionStore = sessionStore;
        }

        public async Task<SeatMapResponseDto> Handle(GetSeatMapQuery request, CancellationToken cancellationToken)
        {
            // Same reasoning as GetFlightAncillariesQueryHandler: Air_GetSeatMap's docs
            // specify the Flight_Key from an Air_Reprice response.
            var legs = await ItineraryReprice.RepriceAsync(
                _sessionStore, _supplierClientResolver, request.OfferId, request.ItineraryOfferIds, cancellationToken);
            var leg = legs.Single(l => l.OfferId == request.OfferId);
            var session = leg.Session;
            var updatedSession = leg.Session;
            var supplierClient = _supplierClientResolver.Resolve(session.SupplierCode);
            var combined = legs.Count > 1 && legs.All(l => l.Session.FlightKey == session.FlightKey);

            var travelers = request.Travelers
                .Select((t, index) => new SupplierPaxDetailDto(
                    index + 1,
                    MapPaxType(t.PaxType),
                    t.Title,
                    t.FirstName,
                    t.LastName,
                    MapGender(t.Gender)))
                .ToList();

            var result = await supplierClient.GetSeatMapAsync(
                new SupplierSeatMapRequestDto(session.SearchKey, updatedSession.FlightKey, travelers),
                cancellationToken);

            var segments = result.Segments
                .Where(seg => !combined || seg.LegIndex == leg.Position)
                .Select(seg => new SeatMapSegmentDto(
                    seg.LegIndex,
                    seg.Rows
                        .Select(row => new SeatMapRowDto(row.Seats
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
                                o.ApplicablePaxTypes,
                                o.SeatRow,
                                o.SeatColumn,
                                o.IsExtraLegroom,
                                o.IsExitRow))
                            .ToList()))
                        .ToList(),
                    seg.Origin,
                    seg.Destination))
                .ToList();

            return new SeatMapResponseDto(segments);
        }

        // 0-ADT/1-CHD/2-INF, matching Flyshop's own FareDetails.PAX_Type convention.
        private static int MapPaxType(string paxType) => paxType switch
        {
            "Child" => 1,
            "Infant" => 2,
            _ => 0
        };

        // 0-Male/1-Female — the only two values Flyshop's PAX_Details documents.
        private static int MapGender(string gender) => gender switch
        {
            "Female" => 1,
            _ => 0
        };
    }
}
