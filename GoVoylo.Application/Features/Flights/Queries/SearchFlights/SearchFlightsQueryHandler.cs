using GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, FlightSearchResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly IFlightSearchResultCache _resultCache;
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly ISender _mediator;

        public SearchFlightsQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore,
            IFlightSearchResultCache resultCache,
            ISearchLogRepository searchLogRepository,
            ISender mediator)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
            _resultCache = resultCache;
            _searchLogRepository = searchLogRepository;
            _mediator = mediator;
        }

        public async Task<FlightSearchResponseDto> Handle(
            SearchFlightsQuery request, CancellationToken cancellationToken)
        {
            // Recorded before the supplier call so history/recent-search/popular-routes
            // reflect what the customer searched for, independent of whether the
            // supplier call succeeds.
            foreach (var segment in request.Request.Segments)
            {
                await _searchLogRepository.AddAsync(new SearchLog(
                    request.UserId,
                    segment.Origin,
                    segment.Destination,
                    segment.TravelDate,
                    request.Request.TripType,
                    request.Request.CabinClass));
            }

            if (request.UserId.HasValue)
            {
                var searchedAirports = request.Request.Segments
                    .SelectMany(s => new[] { s.Origin, s.Destination })
                    .Distinct();

                foreach (var iataCode in searchedAirports)
                {
                    await _mediator.Send(
                        new SaveRecentAirportSearchCommand(request.UserId.Value, iataCode), cancellationToken);
                }
            }

            var result = await _supplierClient.SearchAsync(request.Request, cancellationToken);

            var offers = new List<FlightOfferDto>();

            foreach (var flight in result.Flights)
            {
                var session = new FlightOfferSession(
                    _supplierClient.SupplierCode, result.SearchKey, flight.FlightKey, flight.FareId);

                var offerId = await _sessionStore.SaveAsync(session, cancellationToken);

                offers.Add(MapOffer(offerId, flight));
            }

            var searchId = Guid.NewGuid();
            await _resultCache.SaveAsync(searchId, offers, cancellationToken);

            return new FlightSearchResponseDto(searchId, offers);
        }

        private static FlightOfferDto MapOffer(Guid offerId, SupplierFlightOptionDto flight) => new(
            offerId,
            flight.AirlineCode,
            flight.AirlineName,
            flight.Refundable,
            flight.IsLowCostCarrier,
            flight.Segments
                .Select(s => new FlightOfferSegmentDto(
                    s.Origin, s.Destination, s.AirlineCode, s.FlightNumber,
                    s.DepartureDateTime, s.ArrivalDateTime, s.Duration))
                .ToList(),
            flight.TotalAmount,
            flight.CurrencyCode,
            flight.SeatsAvailable,
            new FareBreakdownDto(
                flight.FareBreakdown.BasicAmount,
                flight.FareBreakdown.AirportTaxAmount,
                flight.FareBreakdown.Taxes
                    .Select(t => new FareTaxDto(t.TaxCode, t.TaxDesc, t.TaxAmount))
                    .ToList(),
                flight.FareBreakdown.ServiceFeeAmount,
                flight.FareBreakdown.TradeMarkupAmount,
                flight.FareBreakdown.PromoDiscount,
                flight.FareBreakdown.Gst,
                flight.FareBreakdown.Tds,
                flight.FareBreakdown.TotalAmount,
                flight.FareBreakdown.CurrencyCode),
            new BaggageDto(flight.Baggage.CheckInBaggage, flight.Baggage.HandBaggage),
            flight.RescheduleCharges
                .Select(r => new RescheduleChargeDto(
                    r.PassengerType, r.Value, r.ValueType, r.DurationFrom, r.DurationTo,
                    r.DurationTypeFrom, r.DurationTypeTo, r.OnlineServiceFee, r.OfflineServiceFee, r.Remarks))
                .ToList());
    }
}
