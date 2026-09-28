using GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, FlightSearchResponseDto>
    {
        private readonly IReadOnlyList<IFlightSupplierClient> _supplierClients;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly ISender _mediator;

        public SearchFlightsQueryHandler(
            IEnumerable<IFlightSupplierClient> supplierClients,
            IFlightSearchSessionStore sessionStore,
            ISender mediator)
        {
            _supplierClients = supplierClients.ToList();
            _sessionStore = sessionStore;
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

            // Fan out to every registered supplier in parallel — one supplier being
            // down or erroring (e.g. Tripjack rejecting a route it doesn't cover)
            // shouldn't take out search results from the others, so each call is
            // wrapped individually rather than awaited under one try/catch.
            var searchTasks = _supplierClients
                .Select(async client =>
                {
                    try
                    {
                        var result = await client.SearchAsync(request.Request, cancellationToken);
                        return (Client: client, Result: result, Error: (Exception?)null);
                    }
                    catch (Exception ex)
                    {
                        return (Client: client, Result: (SupplierFlightSearchResultDto?)null, Error: ex);
                    }
                })
                .ToList();

            var searchOutcomes = await Task.WhenAll(searchTasks);

            var successfulOutcomes = searchOutcomes.Where(o => o.Result != null).ToList();
            if (successfulOutcomes.Count == 0 && searchOutcomes.Length > 0)
            {
                // Every supplier failed — surface the first failure rather than
                // silently returning an empty result set.
                throw searchOutcomes[0].Error!;
            }

            var offers = new List<FlightOfferDto>();

            foreach (var outcome in successfulOutcomes)
            {
                var client = outcome.Client;
                var result = outcome.Result!;

                foreach (var flight in result.Flights)
                {
                    var firstSegment = flight.Segments.FirstOrDefault();
                    var lastSegment = flight.Segments.LastOrDefault();

                    var session = new FlightOfferSession(
                        client.SupplierCode,
                        result.SearchKey,
                        flight.FlightKey,
                        flight.FareId,
                        firstSegment?.Origin ?? string.Empty,
                        lastSegment?.Destination ?? string.Empty,
                        firstSegment?.DepartureDateTime ?? default,
                        flight.AirlineCode,
                        flight.AirlineName,
                        firstSegment?.FlightNumber ?? string.Empty,
                        flight.TotalAmount,
                        flight.CurrencyCode);

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
                                s.AirlineName,
                                s.FlightNumber,
                                s.DepartureDateTime,
                                s.ArrivalDateTime,
                                s.Duration))
                            .ToList(),
                        flight.TotalAmount,
                        flight.CurrencyCode,
                        flight.SeatsAvailable,
                        flight.Fares
                            .Select(f => new FareOptionDto(
                                f.FareId, f.Refundable, f.TotalAmount, f.CurrencyCode, f.CheckInBaggage, f.HandBaggage))
                            .ToList(),
                        flight.TripLegIndex));
                }
            }

            // Merged across suppliers, cheapest first — the natural ordering once more
            // than one supplier can contribute to the same result set.
            var sortedOffers = offers.OrderBy(o => o.TotalAmount).ToList();

            return new FlightSearchResponseDto(sortedOffers);
        }
    }
}
