using GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, FlightSearchResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly IFlightSearchResultCache _resultCache;
        private readonly ISender _mediator;

        public SearchFlightsQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore,
            IFlightSearchResultCache resultCache,
            ISender mediator)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
            _resultCache = resultCache;
            _mediator = mediator;
        }

        public async Task<FlightSearchResponseDto> Handle(
            SearchFlightsQuery request, CancellationToken cancellationToken)
        {
            // Recorded before the supplier call so "recent search" reflects what the
            // customer searched for, independent of whether the supplier call succeeds.
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

                offers.Add(new FlightOfferDto(
                    offerId,
                    flight.AirlineCode,
                    flight.AirlineName,
                    flight.Refundable,
                    flight.IsLowCostCarrier,
                    flight.Segments
                        .Select(s => new FlightOfferSegmentDto(
                            s.Origin,
                            s.Destination,
                            s.AirlineCode,
                            s.FlightNumber,
                            s.DepartureDateTime,
                            s.ArrivalDateTime,
                            s.Duration))
                        .ToList(),
                    flight.TotalAmount,
                    flight.CurrencyCode,
                    flight.SeatsAvailable));
            }

            var searchId = Guid.NewGuid();
            await _resultCache.SaveAsync(searchId, offers, cancellationToken);

            return new FlightSearchResponseDto(searchId, offers);
        }
    }
}
