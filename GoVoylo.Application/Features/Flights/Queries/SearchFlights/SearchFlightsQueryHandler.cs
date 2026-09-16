using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, FlightSearchResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public SearchFlightsQueryHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<FlightSearchResponseDto> Handle(
            SearchFlightsQuery request, CancellationToken cancellationToken)
        {
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

            return new FlightSearchResponseDto(offers);
        }
    }
}
