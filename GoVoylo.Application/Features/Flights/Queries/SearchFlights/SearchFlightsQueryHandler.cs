using GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Pricing.Services;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, FlightSearchResponseDto>
    {
        private readonly IReadOnlyList<IFlightSupplierClient> _supplierClients;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly ISender _mediator;
        private readonly ICommissionDiscountRateRepository _discountRates;

        public SearchFlightsQueryHandler(
            IEnumerable<IFlightSupplierClient> supplierClients,
            IFlightSearchSessionStore sessionStore,
            ISender mediator,
            ICommissionDiscountRateRepository discountRates)
        {
            _supplierClients = supplierClients.ToList();
            _sessionStore = sessionStore;
            _mediator = mediator;
            _discountRates = discountRates;
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
            // Share of each fare's commission given back as discount (CommissionPricing).
            var discountPercent = await _discountRates.GetPercentAtAsync(DateTime.UtcNow, cancellationToken);

            foreach (var outcome in successfulOutcomes)
            {
                var client = outcome.Client;
                var result = outcome.Result!;

                foreach (var flight in result.Flights)
                {
                    var firstSegment = flight.Segments.FirstOrDefault();
                    var lastSegment = flight.Segments.LastOrDefault();
                    var trips = MapTrips(flight.Segments);
                    // The offer's own price is its primary fare's.
                    var primaryFare = flight.Fares.FirstOrDefault(f => f.FareId == flight.FareId) ?? flight.Fares.FirstOrDefault();
                    var offerCommission = primaryFare?.BookingCommissionAmount ?? 0m;

                    var session = new FlightOfferSession(
                        client.SupplierCode,
                        result.SearchKey,
                        flight.FlightKey,
                        flight.FareId,
                        firstSegment?.Origin ?? string.Empty,
                        TripDestination(firstSegment, lastSegment, trips, request.Request.Segments),
                        firstSegment?.DepartureDateTime ?? default,
                        flight.AirlineCode,
                        flight.AirlineName,
                        firstSegment?.FlightNumber ?? string.Empty,
                        flight.TotalAmount,
                        flight.CurrencyCode,
                        trips,
                        request.Request.TripType,
                        flight.Fares
                            .Select(f => new FlightOfferSessionFare(
                                f.FareId, f.AdultBaseFare, f.ChildBaseFare,
                                f.BookingCommissionAmount,
                                CommissionPricing.Discount(f.BookingCommissionAmount, discountPercent),
                                f.AdultListFare,
                                f.ChildListFare))
                            .ToList());

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
                                s.Duration,
                                s.TripIndex))
                            .ToList(),
                        flight.TotalAmount + CommissionPricing.Retained(offerCommission, discountPercent),
                        flight.CurrencyCode,
                        flight.SeatsAvailable,
                        flight.Fares.Select(f => PriceFare(f, discountPercent)).ToList(),
                        flight.TripLegIndex,
                        client.SupplierCode,
                        flight.TotalAmount + offerCommission,
                        CommissionPricing.Discount(offerCommission, discountPercent)));
                }
            }

            // Merged across suppliers, cheapest first — the natural ordering once more
            // than one supplier can contribute to the same result set.
            var sortedOffers = offers.OrderBy(o => o.TotalAmount).ToList();

            return new FlightSearchResponseDto(sortedOffers);
        }

        // The supplier's fare with our commission added and the discount taken off:
        // what the app shows (list price struck out) and charges.
        private static FareOptionDto PriceFare(SupplierFareOptionDto f, decimal discountPercent)
        {
            var bookingSupplierAmount = f.BookingTotalAmount > 0 ? f.BookingTotalAmount : f.TotalAmount;
            return new FareOptionDto(
                f.FareId,
                f.Refundable,
                f.TotalAmount + CommissionPricing.Retained(f.AdultCommission, discountPercent),
                f.CurrencyCode,
                f.CheckInBaggage,
                f.HandBaggage,
                bookingSupplierAmount + CommissionPricing.Retained(f.BookingCommissionAmount, discountPercent),
                f.FareIdentifier,
                f.SpecialReturnId,
                f.MatchingSpecialReturnIds ?? Array.Empty<string>(),
                f.BookingBaseAmount,
                f.AdultBaseFare,
                f.ChildBaseFare,
                ListAmount: f.TotalAmount + f.AdultCommission,
                DiscountAmount: CommissionPricing.Discount(f.AdultCommission, discountPercent),
                BookingListAmount: bookingSupplierAmount + f.BookingCommissionAmount,
                BookingDiscountAmount: CommissionPricing.Discount(f.BookingCommissionAmount, discountPercent),
                BookingCommissionAmount: f.BookingCommissionAmount,
                AdultListFare: f.AdultListFare,
                ChildListFare: f.ChildListFare);
        }

        // Where the booking's single leg is recorded as going. A combined out-and-back
        // fare (Flyshop Booking_Type 2 leaves every segment's TripIndex 0, so MapTrips
        // can't split it) starts and ends at the same airport; it's recorded against
        // its turnaround point — the first searched leg's destination — so My Trips
        // reads DEL → GOI rather than DEL → DEL. Everything else keeps its last arrival.
        private static string TripDestination(
            SupplierFlightSegmentDto? firstSegment,
            SupplierFlightSegmentDto? lastSegment,
            IReadOnlyList<FlightOfferSessionTrip>? trips,
            IReadOnlyList<FlightSearchSegmentDto> searchedLegs)
        {
            var destination = lastSegment?.Destination ?? string.Empty;
            if (trips == null
                && firstSegment != null
                && searchedLegs.Count > 1
                && string.Equals(destination, firstSegment.Origin, StringComparison.OrdinalIgnoreCase))
            {
                return searchedLegs[0].Destination;
            }
            return destination;
        }

        // One entry per trip of a whole-trip offer (see FlightOfferSession.Trips);
        // null for an ordinary single-trip offer.
        private static IReadOnlyList<FlightOfferSessionTrip>? MapTrips(IReadOnlyList<SupplierFlightSegmentDto> segments)
        {
            var trips = segments
                .GroupBy(s => s.TripIndex)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var first = g.First();
                    return new FlightOfferSessionTrip(
                        first.Origin,
                        g.Last().Destination,
                        first.DepartureDateTime,
                        first.AirlineCode,
                        first.AirlineName,
                        first.FlightNumber);
                })
                .ToList();

            return trips.Count > 1 ? trips : null;
        }
    }
}
