using System.Text.Json.Serialization;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    // Every wire model below has been verified against a real, live response from
    // Tripjack's UAT environment (apitest.tripjack.com), not just their docs —
    // the previous version of this file was built from field-table descriptions
    // alone and used the wrong auth model and endpoint names entirely. Field names
    // are Tripjack's own short/minified keys (e.g. "sI" = segment info, "fD" =
    // flight designator, "fd" = fare details — same abbreviation, different meaning,
    // exactly as Tripjack's own API returns it).

    // ===== Search (POST fms/v1/air-search-all) =====

    public class TripjackSearchRequestWire
    {
        [JsonPropertyName("searchQuery")]
        public TripjackSearchQueryWire SearchQuery { get; set; } = new();
    }

    public class TripjackSearchQueryWire
    {
        [JsonPropertyName("cabinClass")]
        public string CabinClass { get; set; } = "ECONOMY";

        [JsonPropertyName("paxInfo")]
        public TripjackPaxInfoWire PaxInfo { get; set; } = new();

        [JsonPropertyName("routeInfos")]
        public List<TripjackRouteInfoWire> RouteInfos { get; set; } = new();
    }

    public class TripjackPaxInfoWire
    {
        [JsonPropertyName("ADULT")]
        public int Adult { get; set; }

        [JsonPropertyName("CHILD")]
        public int Child { get; set; }

        [JsonPropertyName("INFANT")]
        public int Infant { get; set; }
    }

    public class TripjackRouteInfoWire
    {
        [JsonPropertyName("fromCityOrAirport")]
        public TripjackAirportCodeWire FromCityOrAirport { get; set; } = new();

        [JsonPropertyName("toCityOrAirport")]
        public TripjackAirportCodeWire ToCityOrAirport { get; set; } = new();

        // YYYY-MM-DD.
        [JsonPropertyName("travelDate")]
        public string TravelDate { get; set; } = string.Empty;
    }

    public class TripjackAirportCodeWire
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }

    public class TripjackSearchResponseWire
    {
        [JsonPropertyName("searchResult")]
        public TripjackSearchResultWire? SearchResult { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }
    }

    public class TripjackStatusWire
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("httpStatus")]
        public int HttpStatus { get; set; }
    }

    public class TripjackSearchResultWire
    {
        // Keyed by "ONWARD"/"RETURN" for a domestic return, "COMBO" for
        // international-return/multi-city, or indexed keys for a domestic
        // multi-city (2-6 legs) — see Tripjack's own "Journey Types & Response
        // Structure" table. Order of Dictionary insertion follows the JSON's own
        // property order, which is what TripLegIndex is derived from below.
        [JsonPropertyName("tripInfos")]
        public Dictionary<string, List<TripjackTripOptionWire>> TripInfos { get; set; } = new();
    }

    public class TripjackTripOptionWire
    {
        [JsonPropertyName("sI")]
        public List<TripjackSegmentInfoWire> SegmentInfos { get; set; } = new();

        // Multiple entries here are different fare tiers (PUBLISHED/SPECIAL_RETURN/
        // TJ_FLEX, or different fare baskets) for the SAME physical flight above —
        // mapped as this flight's own Fares list, same shape Flyshop already uses.
        [JsonPropertyName("totalPriceList")]
        public List<TripjackPriceWire> TotalPriceList { get; set; } = new();
    }

    public class TripjackSegmentInfoWire
    {
        [JsonPropertyName("fD")]
        public TripjackFlightDesignatorWire FlightDesignator { get; set; } = new();

        [JsonPropertyName("stops")]
        public int Stops { get; set; }

        [JsonPropertyName("duration")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("da")]
        public TripjackAirportDetailWire Departure { get; set; } = new();

        [JsonPropertyName("aa")]
        public TripjackAirportDetailWire Arrival { get; set; } = new();

        // Local wall-clock, e.g. "2026-10-25T06:30" — no timezone offset given.
        [JsonPropertyName("dt")]
        public string DepartureDateTime { get; set; } = string.Empty;

        [JsonPropertyName("at")]
        public string ArrivalDateTime { get; set; } = string.Empty;
    }

    public class TripjackFlightDesignatorWire
    {
        [JsonPropertyName("aI")]
        public TripjackAirlineInfoWire AirlineInfo { get; set; } = new();

        [JsonPropertyName("fN")]
        public string FlightNumber { get; set; } = string.Empty;
    }

    public class TripjackAirlineInfoWire
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("isLcc")]
        public bool IsLcc { get; set; }
    }

    public class TripjackAirportDetailWire
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class TripjackPriceWire
    {
        // This IS the priceId — pass verbatim to Review. Tripjack has no separate
        // Flight_Key/Fare_Id split the way Flyshop does; one opaque id covers both,
        // so this same value is used for both SupplierFlightOptionDto.FlightKey and
        // SupplierFareOptionDto.FareId (see TripjackClient.MapFlight).
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("fareIdentifier")]
        public string? FareIdentifier { get; set; }

        // Keyed by pax type ("ADULT"/"CHILD"/"INFANT").
        [JsonPropertyName("fd")]
        public Dictionary<string, TripjackFareDetailWire> FareDetailsByPaxType { get; set; } = new();
    }

    public class TripjackFareDetailWire
    {
        [JsonPropertyName("fC")]
        public TripjackFareComponentWire FareComponent { get; set; } = new();

        [JsonPropertyName("sR")]
        public int SeatsRemaining { get; set; }

        [JsonPropertyName("bI")]
        public TripjackBaggageInfoWire? BaggageInfo { get; set; }

        // 0-Non-refundable, 1-Refundable, 2-Partially refundable.
        [JsonPropertyName("rT")]
        public int RefundableType { get; set; }

        [JsonPropertyName("cc")]
        public string? CabinClass { get; set; }
    }

    public class TripjackFareComponentWire
    {
        // Base Fare.
        [JsonPropertyName("BF")]
        public decimal BaseFare { get; set; }

        // Total Fare — what the agent is actually charged.
        [JsonPropertyName("TF")]
        public decimal TotalFare { get; set; }

        // Taxes and Fees.
        [JsonPropertyName("TAF")]
        public decimal TaxesAndFees { get; set; }
    }

    public class TripjackBaggageInfoWire
    {
        [JsonPropertyName("iB")]
        public string? CheckInBaggage { get; set; }

        [JsonPropertyName("cB")]
        public string? CabinBaggage { get; set; }
    }

    // ===== Review (POST fms/v1/review) =====
    //
    // Tripjack's booking flow has no separate "reprice" step the way Flyshop does —
    // Review both re-validates price/availability AND is the one call that produces
    // the bookingId every later step (Seat Map, Book, Confirm-Book, Fare Rules,
    // Booking Details) needs. IFlightSupplierClient.RepriceAsync is the closest
    // existing seam in our own interface, so TripjackClient.RepriceAsync calls
    // Review and returns the resulting bookingId packed into
    // SupplierRepriceResultDto.FlightKey — every later Tripjack call in this client
    // reads it back out of there rather than the priceId, since bookingId is what
    // Tripjack itself expects from this point on.

    public class TripjackReviewRequestWire
    {
        [JsonPropertyName("priceIds")]
        public List<string> PriceIds { get; set; } = new();
    }

    public class TripjackReviewResponseWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;

        [JsonPropertyName("totalPriceInfo")]
        public TripjackTotalPriceInfoWire? TotalPriceInfo { get; set; }

        [JsonPropertyName("conditions")]
        public TripjackConditionsWire? Conditions { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }
    }

    public class TripjackTotalPriceInfoWire
    {
        [JsonPropertyName("totalFareDetail")]
        public TripjackFareComponentWire? TotalFareDetail { get; set; }
    }

    public class TripjackConditionsWire
    {
        // Hold (hold without payment) allowed for this fare.
        [JsonPropertyName("isBA")]
        public bool IsHoldAllowed { get; set; }

        // Call Seat Map only when true.
        [JsonPropertyName("isa")]
        public bool IsSeatApplicable { get; set; }

        // Session time in seconds — how long this bookingId stays valid.
        [JsonPropertyName("st")]
        public int SessionTimeSeconds { get; set; }
    }

    // ===== Everything past Review is still NOT implemented =====
    //
    // Real endpoints and shapes (confirmed from Tripjack's own docs, NOT yet
    // verified against a live response the way Search/Review above are — treat the
    // field names below as a starting point to confirm, not ground truth):
    //
    //   Seat Map            POST fms/v1/seat                          { bookingId }
    //   Fare Rule           POST fms/v2/farerule                      { flowType: SEARCH|REVIEW|BOOKING_DETAIL, id }
    //   Fare Validate       POST oms/v1/air/book/fare-validate        same traveller/SSR shape as Book
    //   Book (Instant/Hold) POST oms/v1/air/book                      { bookingId, paymentInfos?[{amount}], deliveryInfo, travellerInfo[] }
    //   Confirm Fare        POST oms/v1/air/fare-validate             (hold -> validate before ticketing)
    //   Confirm-Book        POST oms/v1/air/confirm-book              { bookingId, paymentInfos[{amount}], ... }
    //   Booking Details     POST oms/v1/booking-details               call ~5s after Book/Confirm-Book, not immediately
    //   Release PNR (Hold)  POST oms/v1/air/unhold                    { bookingId } (unconfirmed field name)
    //   Get Amendment Charges POST oms/v1/air/amendment/amendment-charges  (quote — cancellation has a charge, not a flat call)
    //   Submit Amendment    POST oms/v1/air/amendment/submit-amendment    (commit — two-step cancellation, structurally
    //                                                                      different from Flyshop's single Air_TicketCancellation)
    //
    // CreateTempBookingAsync/CreateBlockTicketAsync/AddPaymentAsync/BookTicketAsync/
    // CancelBookingAsync/ReleaseHoldAsync/GetFareRulesAsync/GetSeatMapAsync/
    // GetAncillariesAsync all still throw NotSupportedException in TripjackClient
    // until these are implemented and live-verified the same way Search/Review were.
}
