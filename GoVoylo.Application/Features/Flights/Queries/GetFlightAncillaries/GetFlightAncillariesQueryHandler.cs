using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries
{
    public class GetFlightAncillariesQueryHandler
        : IRequestHandler<GetFlightAncillariesQuery, FlightAncillariesResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public GetFlightAncillariesQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<FlightAncillariesResponseDto> Handle(
            GetFlightAncillariesQuery request, CancellationToken cancellationToken)
        {
            var session = await _sessionStore.GetAsync(request.OfferId, cancellationToken);

            if (session == null)
            {
                throw new NotFoundException("Flight offer not found or has expired. Please search again.");
            }

            // Air_GetSSR's own docs specify using the Flight_Key from an Air_Reprice
            // response, not the one from Air_Search — reprice here rather than assume
            // the search-time key is still valid, same as RepriceFlightOfferQueryHandler.
            var repriceResult = await _supplierClient.RepriceAsync(
                new SupplierRepriceRequestDto(session.SearchKey, session.FlightKey, session.FareId),
                cancellationToken);

            var updatedSession = session with
            {
                FlightKey = repriceResult.FlightKey,
                FareId = repriceResult.FareId
            };
            await _sessionStore.UpdateAsync(request.OfferId, updatedSession, cancellationToken);

            var result = await _supplierClient.GetAncillariesAsync(
                new SupplierAncillaryRequestDto(session.SearchKey, updatedSession.FlightKey),
                cancellationToken);

            var options = result.Options
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
