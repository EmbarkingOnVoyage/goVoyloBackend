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

            if (wireResponse.Status?.Success != true || string.IsNullOrEmpty(wireResponse.BookingId))
            {
                throw new InvalidOperationException("Tripjack Review returned no bookingId.");
            }

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

        public async Task<SupplierTempBookingResultDto> CreateTempBookingAsync(
            SupplierTempBookingRequestDto request, CancellationToken cancellationToken)
        {
            if (request.Flights.Count != 1)
            {
                // Each leg goes through its own separate Review call today (one
                // RepriceAsync call per leg — see CreateBookingCommandHandler), each
                // producing its own bookingId. Tripjack's own model expects every
                // leg's priceId submitted together in ONE Review call to get a single
                // bookingId covering a multi-leg itinerary, so a roundtrip/multi-city
                // Tripjack booking isn't safe to attempt until that's reworked.
                throw new NotSupportedException(
                    "Tripjack booking only supports oneway itineraries today — a multi-leg itinerary needs every " +
                    "leg's priceId submitted together in one Review call, which the current per-leg reprice flow doesn't do.");
            }

            // RepriceAsync (Review) already ran for this leg and its bookingId is
            // carried here as FlightKey — see RepriceAsync's own doc comment.
            var bookingId = request.Flights[0].FlightKey;

            var wireRequest = new TripjackBookRequestWire
            {
                BookingId = bookingId,
                DeliveryInfo = new TripjackDeliveryInfoWire
                {
                    Emails = new List<string> { request.PassengerEmail },
                    Contacts = new List<string> { NormalizeMobile(request.PassengerMobile) }
                },
                TravellerInfo = request.Travelers.Select(MapTraveller).ToList()
                // PaymentInfos intentionally omitted — this is what makes it a Hold
                // rather than an Instant Book.
            };

            var wireResponse = await PostAsync<TripjackBookRequestWire, TripjackBookResponseWire>(
                "oms/v1/air/book", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Book");

            return new SupplierTempBookingResultDto(wireResponse.BookingId ?? bookingId);
        }

        public async Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            // Tripjack's own integration guide: Booking Details has to be called
            // "after 5 seconds elapsed" — the PNR/ticket data isn't populated in the
            // Book response itself, confirmed live (Book's own response is just an
            // echo of bookingId + status).
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var wireResponse = await PostAsync<TripjackBookingDetailsRequestWire, TripjackBookingDetailsResponseWire>(
                "oms/v1/booking-details",
                new TripjackBookingDetailsRequestWire { BookingId = bookingRefNo },
                cancellationToken);

            var order = wireResponse.Order;
            var airInfo = wireResponse.ItemInfos?.Air;
            var traveller = airInfo?.TravellerInfos.FirstOrDefault();
            var pnr = traveller?.PnrDetails.Values.FirstOrDefault();
            var statusId = MapOrderStatus(order?.Status);

            var legs = (airInfo?.TripInfos ?? new List<TripjackTripOptionWire>())
                .SelectMany(t => t.SegmentInfos)
                .Select(seg => new SupplierTicketingLegResultDto(
                    // No single stable per-segment id the way Flyshop's Flight_Id is
                    // (Booking Details' own segment id changes between Search/Review/
                    // Book, confirmed live) — the route itself is the one identifier
                    // that stays meaningful across calls.
                    $"{seg.Departure.Code}-{seg.Arrival.Code}",
                    statusId,
                    seg.FlightDesignator.AirlineInfo.Code,
                    pnr,
                    null,
                    null,
                    null))
                .ToList();

            return new SupplierTicketingResultDto(
                order?.BookingId ?? bookingRefNo,
                statusId,
                legs.FirstOrDefault()?.AirlineCode,
                pnr,
                null,
                null,
                null,
                legs);
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

        public async Task ReleaseHoldAsync(
            SupplierReleaseHoldRequestDto request, CancellationToken cancellationToken)
        {
            // Confirmed live: bookingId alone is rejected (errCode 1072,
            // "Cancellation not available for PNR") — the pnrs array is required too.
            var wireResponse = await PostAsync<TripjackUnholdRequestWire, TripjackStatusOnlyResponseWire>(
                "oms/v1/air/unhold",
                new TripjackUnholdRequestWire
                {
                    BookingId = request.BookingRefNo,
                    Pnrs = new List<string> { request.AirlinePnr }
                },
                cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Unhold");
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

        // Tripjack's own Order Status values, mapped onto the same "11-Success/
        // 22-Failed/33-Block" string convention Flyshop's Status_Id already uses —
        // every existing handler (CancelTripBookingCommandHandler,
        // VerifyRazorpayPaymentCommandHandler, etc.) branches on those literal
        // strings, so reusing them here means a Tripjack booking works with that
        // logic unchanged rather than needing every call site to become
        // supplier-aware about status semantics too.
        private static string MapOrderStatus(string? status) => status switch
        {
            "SUCCESS" => "11",
            "ON_HOLD" => "33",
            "PENDING" => "33",
            _ => "22"
        };

        private static TripjackTravellerInfoWire MapTraveller(SupplierTempBookingPaxDto pax) => new()
        {
            Title = MapTitle(pax.PaxType, pax.Gender),
            PaxType = MapPaxType(pax.PaxType),
            FirstName = pax.FirstName,
            LastName = pax.LastName,
            DateOfBirth = pax.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        // 0-ADT/1-CHD/2-INF, the same convention SupplierTempBookingPaxDto's own
        // callers already use for Flyshop.
        private static string MapPaxType(int paxType) => paxType switch
        {
            1 => "CHILD",
            2 => "INFANT",
            _ => "ADULT"
        };

        // Tripjack's own docs: Adult titles are Mr/Mrs/Ms, Child/Infant titles are
        // Ms/Master — Gender (0-Male/1-Female) is all SupplierTempBookingPaxDto
        // carries, so a female adult maps to Mrs rather than distinguishing Ms.
        private static string MapTitle(int paxType, int gender)
        {
            var isFemale = gender == 1;
            return paxType switch
            {
                1 or 2 => isFemale ? "Ms" : "Master",
                _ => isFemale ? "Mrs" : "Mr"
            };
        }

        // Tripjack's docs: "contact numbers with country code e.g. +919500112233".
        private static string NormalizeMobile(string mobile) =>
            mobile.StartsWith('+') ? mobile : $"+91{mobile}";

        private static void EnsureSuccess(TripjackStatusWire? status, List<TripjackErrorWire>? errors, string method)
        {
            if (status?.Success == true)
            {
                return;
            }

            var detail = errors == null || errors.Count == 0
                ? string.Empty
                : $" ({string.Join("; ", errors.Select(e => $"{e.ErrorCode}: {e.Message}"))})";

            throw new InvalidOperationException($"Tripjack {method} failed{detail}");
        }

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
