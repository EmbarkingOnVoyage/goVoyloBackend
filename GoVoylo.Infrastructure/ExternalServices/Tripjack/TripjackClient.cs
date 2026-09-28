using System.Globalization;
using System.Net.Http.Json;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    public class TripjackClient : IFlightSupplierClient
    {
        private readonly HttpClient _httpClient;

        public TripjackClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string SupplierCode => FlightSupplierCodes.Tripjack;

        public async Task<SupplierFlightSearchResultDto> SearchAsync(
            FlightSearchRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new TripjackSearchRequestWire
            {
                SearchQuery = new TripjackSearchQueryWire
                {
                    CabinClass = MapCabinClass(request.CabinClass),
                    PaxInfo = new TripjackPaxInfoWire
                    {
                        Adult = request.AdultCount,
                        Child = request.ChildCount,
                        Infant = request.InfantCount
                    },
                    RouteInfos = request.Segments
                        .Select(s => new TripjackRouteInfoWire
                        {
                            FromCityOrAirport = new TripjackAirportCodeWire { Code = s.Origin },
                            ToCityOrAirport = new TripjackAirportCodeWire { Code = s.Destination },
                            TravelDate = s.TravelDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        })
                        .ToList()
                }
            };

            var wireResponse = await PostAsync<TripjackSearchRequestWire, TripjackSearchResponseWire>(
                "fms/v1/air-search-all", wireRequest, cancellationToken);

            var tripInfos = wireResponse.SearchResult?.TripInfos ?? new Dictionary<string, List<TripjackTripOptionWire>>();

            // Dictionary insertion order follows the JSON's own key order (ONWARD
            // first, then RETURN for a domestic return) — same TripLegIndex meaning
            // Flyshop's TripDetails[].Trip_Id already carries for a multi-leg search.
            var flights = tripInfos.Values
                .SelectMany((tripOptions, legIndex) => tripOptions.Select(option => MapFlight(option, legIndex)))
                .ToList();

            // Tripjack has no equivalent of Flyshop's Search_Key — Review only ever
            // needs the priceId (carried as FlightKey/FareId below), so this is left
            // empty and never read back for a Tripjack offer.
            return new SupplierFlightSearchResultDto(string.Empty, flights);
        }

        public async Task<SupplierRepriceResultDto> RepriceAsync(
            SupplierRepriceRequestDto request, CancellationToken cancellationToken)
        {
            // Tripjack's Review is both "reprice" and "start a booking session" in one
            // call — there's no separate Flight_Key vs Fare_Id the way Flyshop has, so
            // FareId is ignored here and FlightKey (the priceId from search) is the
            // only thing Review needs.
            var wireRequest = new TripjackReviewRequestWire
            {
                PriceIds = new List<string> { request.FlightKey }
            };

            var wireResponse = await PostAsync<TripjackReviewRequestWire, TripjackReviewResponseWire>(
                "fms/v1/review", wireRequest, cancellationToken);

            var totalFare = wireResponse.TotalPriceInfo?.TotalFareDetail?.TotalFare ?? 0m;

            // Every later Tripjack call (Seat Map, Book, Confirm-Book, Fare Rules,
            // Booking Details) needs this bookingId, not the original priceId — it's
            // carried forward as FlightKey since that's the field every caller of
            // RepriceAsync already threads through to the next step.
            return new SupplierRepriceResultDto(
                wireResponse.BookingId,
                request.FareId,
                totalFare,
                "INR",
                Repriced: true,
                IsFareChange: false);
        }

        public Task<SupplierLowFareResultDto> GetLowFareCalendarAsync(
            SupplierLowFareRequestDto request, CancellationToken cancellationToken)
        {
            // Confirmed absent from Tripjack's Flights API v2.0 docs — no fare
            // calendar endpoint exists for this supplier at all.
            throw new NotSupportedException("Tripjack does not support a low-fare calendar.");
        }

        public Task<SupplierAncillaryResultDto> GetAncillariesAsync(
            SupplierAncillaryRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint exists (Review's own sI[].ssrInfo already returns MEAL/
            // BAGGAGE options inline — confirmed live), but nothing here maps that
            // response into SupplierAncillaryResultDto yet.
            throw new NotSupportedException("Tripjack ancillary services are not implemented.");
        }

        public Task<SupplierSeatMapResultDto> GetSeatMapAsync(
            SupplierSeatMapRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint: POST fms/v1/seat, { bookingId }. See
            // TripjackWireModels.cs's own notes — not yet implemented/verified.
            throw new NotSupportedException("Tripjack seat map is not implemented.");
        }

        public Task<SupplierTempBookingResultDto> CreateTempBookingAsync(
            SupplierTempBookingRequestDto request, CancellationToken cancellationToken)
        {
            // Tripjack has no separate "temp booking" step distinct from Review
            // (already called via RepriceAsync above) — this would need to become a
            // thin pass-through once Book/Confirm-Book exist, not its own HTTP call.
            throw new NotSupportedException("Tripjack booking is not implemented.");
        }

        public Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            // Real endpoint: POST oms/v1/air/book WITHOUT paymentInfos (Hold mode) —
            // only valid when Review's own conditions.isBA was true.
            throw new NotSupportedException("Tripjack ticketing is not implemented.");
        }

        public Task CancelBookingAsync(
            SupplierCancellationRequestDto request, CancellationToken cancellationToken)
        {
            // Real flow is two calls, not one: POST
            // oms/v1/air/amendment/amendment-charges to quote what the customer owes,
            // then POST oms/v1/air/amendment/submit-amendment to commit — Flyshop's
            // single Air_TicketCancellation call has no equivalent here, so
            // CancelBookingAsync's own single-call contract may need to change (or
            // internally do both calls) once this is implemented.
            throw new NotSupportedException("Tripjack cancellation is not implemented.");
        }

        public Task ReleaseHoldAsync(
            SupplierReleaseHoldRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint: POST oms/v1/air/unhold. Exact request field name(s) not
            // yet confirmed against a live response.
            throw new NotSupportedException("Tripjack hold release is not implemented.");
        }

        public Task<SupplierPaymentResultDto> AddPaymentAsync(
            string bookingRefNo, string clientRefNo, CancellationToken cancellationToken)
        {
            // Tripjack has no separate wallet-debit call the way Flyshop's AddPayment
            // is — Confirm-Book (see BookTicketAsync) takes paymentInfos.amount
            // directly, committing payment and ticketing together in one call. This
            // method likely becomes a no-op or a Fare-Validate check rather than a
            // real charge once BookTicketAsync is implemented.
            throw new NotSupportedException("Tripjack payment is not implemented.");
        }

        public Task<SupplierTicketingResultDto> BookTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            // Real endpoint: POST oms/v1/air/confirm-book, { bookingId,
            // paymentInfos: [{ amount }], ... } — this is where Tripjack actually
            // commits payment AND ticketing in a single call, unlike Flyshop's
            // separate AddPayment + Book_Ticket steps.
            throw new NotSupportedException("Tripjack ticketing is not implemented.");
        }

        public Task<SupplierFareRuleResultDto> GetFareRulesAsync(
            SupplierFareRuleRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint: POST fms/v2/farerule, { flowType: "REVIEW", id: bookingId }.
            throw new NotSupportedException("Tripjack fare rules are not implemented.");
        }

        private static SupplierFlightOptionDto MapFlight(TripjackTripOptionWire tripOption, int tripLegIndex)
        {
            var primaryPrice = tripOption.TotalPriceList.FirstOrDefault();
            var adultFare = GetAdultFareDetail(primaryPrice);

            var fares = tripOption.TotalPriceList
                .Select(price =>
                {
                    var fareDetail = GetAdultFareDetail(price);
                    return new SupplierFareOptionDto(
                        price.Id,
                        fareDetail?.RefundableType != 0,
                        fareDetail?.FareComponent.TotalFare ?? 0m,
                        "INR",
                        fareDetail?.BaggageInfo?.CheckInBaggage,
                        fareDetail?.BaggageInfo?.CabinBaggage);
                })
                .ToList();

            var firstSegment = tripOption.SegmentInfos.FirstOrDefault();

            return new SupplierFlightOptionDto(
                // No separate Flight_Key/Fare_Id split for Tripjack — the priceId
                // covers both (see TripjackPriceWire.Id's own doc comment).
                primaryPrice?.Id ?? string.Empty,
                primaryPrice?.Id ?? string.Empty,
                firstSegment?.FlightDesignator.AirlineInfo.Code ?? string.Empty,
                firstSegment?.FlightDesignator.AirlineInfo.Name ?? string.Empty,
                adultFare?.RefundableType != 0,
                firstSegment?.FlightDesignator.AirlineInfo.IsLcc ?? false,
                tripOption.SegmentInfos.Select(MapSegment).ToList(),
                adultFare?.FareComponent.TotalFare ?? 0m,
                "INR",
                adultFare?.SeatsRemaining ?? 0,
                fares,
                tripLegIndex);
        }

        private static TripjackFareDetailWire? GetAdultFareDetail(TripjackPriceWire? price) =>
            price?.FareDetailsByPaxType.TryGetValue("ADULT", out var detail) == true ? detail : null;

        private static SupplierFlightSegmentDto MapSegment(TripjackSegmentInfoWire segment) => new(
            segment.Departure.Code,
            segment.Arrival.Code,
            segment.FlightDesignator.AirlineInfo.Code,
            segment.FlightDesignator.AirlineInfo.Name,
            segment.FlightDesignator.FlightNumber,
            ParseDateTime(segment.DepartureDateTime),
            ParseDateTime(segment.ArrivalDateTime),
            segment.DurationMinutes.ToString(CultureInfo.InvariantCulture));

        private static string MapCabinClass(string cabinClass) => cabinClass switch
        {
            "Business" => "BUSINESS",
            "First" => "FIRST",
            "PremiumEconomy" => "PREMIUM_ECONOMY",
            _ => "ECONOMY"
        };

        private static DateTime ParseDateTime(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : default;

        private async Task<TResponse> PostAsync<TRequest, TResponse>(
            string method, TRequest body, CancellationToken cancellationToken)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(method, body, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var result = await httpResponse.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

            return result ?? throw new InvalidOperationException($"Tripjack {method} returned an empty response.");
        }
    }
}
