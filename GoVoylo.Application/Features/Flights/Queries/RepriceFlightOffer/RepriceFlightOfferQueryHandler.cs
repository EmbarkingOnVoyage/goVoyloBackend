using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.RepriceFlightOffer
{
    public class RepriceFlightOfferQueryHandler
        : IRequestHandler<RepriceFlightOfferQuery, FlightRepriceResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public RepriceFlightOfferQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<FlightRepriceResponseDto> Handle(
            RepriceFlightOfferQuery request, CancellationToken cancellationToken)
        {
            var session = await _sessionStore.GetAsync(request.OfferId, cancellationToken);

            if (session == null)
            {
                throw new NotFoundException("Flight offer not found or has expired. Please search again.");
            }

            var repriceRequest = new SupplierRepriceRequestDto(
                session.SearchKey, session.FlightKey, session.FareId);

            var result = await _supplierClient.RepriceAsync(repriceRequest, cancellationToken);

            var updatedSession = session with { FlightKey = result.FlightKey, FareId = result.FareId };
            await _sessionStore.UpdateAsync(request.OfferId, updatedSession, cancellationToken);

            return new FlightRepriceResponseDto(
                request.OfferId, result.TotalAmount, result.CurrencyCode, result.IsFareChange);
        }
    }
}
