using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetSeatMap
{
    public class GetSeatMapQueryHandler : IRequestHandler<GetSeatMapQuery, SeatMapResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetSeatMapQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<SeatMapResponseDto> Handle(GetSeatMapQuery request, CancellationToken cancellationToken)
        {
            var session = await _sessionStore.GetAsync(request.OfferId, cancellationToken);

            if (session == null)
            {
                throw new NotFoundException("Flight offer not found or has expired. Please search again.");
            }

            // Same reasoning as GetFlightAncillariesQueryHandler: Air_GetSeatMap's docs
            // specify the Flight_Key from an Air_Reprice response.
            var repriceResult = await _supplierClient.RepriceAsync(
                new SupplierRepriceRequestDto(session.SearchKey, session.FlightKey, session.FareId),
                cancellationToken);

            var updatedSession = session with
            {
                FlightKey = repriceResult.FlightKey,
                FareId = repriceResult.FareId
            };
            await _sessionStore.UpdateAsync(request.OfferId, updatedSession, cancellationToken);

            var travelers = request.Travelers
                .Select((t, index) => new SupplierPaxDetailDto(
                    index + 1,
                    MapPaxType(t.PaxType),
                    t.Title,
                    t.FirstName,
                    t.LastName,
                    MapGender(t.Gender)))
                .ToList();

            var result = await _supplierClient.GetSeatMapAsync(
                new SupplierSeatMapRequestDto(session.SearchKey, updatedSession.FlightKey, travelers),
                cancellationToken);

            var segments = result.Segments
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
                                o.ApplicablePaxTypes))
                            .ToList()))
                        .ToList()))
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
