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

    // ===== Book (POST oms/v1/air/book) =====
    //
    // Same request shape for both Instant Book and Hold — omitting paymentInfos is
    // what makes it a Hold (only valid when Review's conditions.isBA was true).
    // Confirm-Book (POST oms/v1/air/confirm-book, ticketing a held booking) reuses
    // this exact same wire shape with paymentInfos populated — see
    // TripjackConfirmBookRequestWire below, a thin alias for that reason.

    public class TripjackBookRequestWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;

        [JsonPropertyName("deliveryInfo")]
        public TripjackDeliveryInfoWire DeliveryInfo { get; set; } = new();

        [JsonPropertyName("travellerInfo")]
        public List<TripjackTravellerInfoWire> TravellerInfo { get; set; } = new();

        // Omit entirely for Hold. Present (with Amount = Review's Total Fare) for
        // Instant Book or Confirm-Book — Tripjack determines the payment medium
        // itself; nothing else about how the agent pays goes in this request.
        [JsonPropertyName("paymentInfos")]
        public List<TripjackPaymentInfoWire>? PaymentInfos { get; set; }
    }

    public class TripjackPaymentInfoWire
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class TripjackDeliveryInfoWire
    {
        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; } = new();

        [JsonPropertyName("contacts")]
        public List<string> Contacts { get; set; } = new();
    }

    public class TripjackTravellerInfoWire
    {
        // Adult: Mr/Mrs/Ms. Child/Infant: Ms/Master.
        [JsonPropertyName("ti")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("pt")]
        public string PaxType { get; set; } = string.Empty;

        [JsonPropertyName("fN")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lN")]
        public string LastName { get; set; } = string.Empty;

        // YYYY-MM-DD — mandatory for INFANT.
        [JsonPropertyName("dob")]
        public string? DateOfBirth { get; set; }
    }

    // The live response is minimal — just an echo of bookingId and status; the real
    // PNR/ticket/fare detail only shows up from Booking Details, called ~5s later.
    public class TripjackBookResponseWire
    {
        [JsonPropertyName("bookingId")]
        public string? BookingId { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    public class TripjackErrorWire
    {
        [JsonPropertyName("errCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    // ===== Confirm-Book (POST oms/v1/air/confirm-book) =====
    // Same wire shape as Book, just always carrying paymentInfos — tickets an
    // already-held booking (call Fare Validate first to confirm the fare still
    // stands). Kept as a separate type alias for readability at call sites even
    // though the JSON shape is identical to TripjackBookRequestWire.
    public class TripjackConfirmBookRequestWire : TripjackBookRequestWire
    {
    }

    // ===== Booking Details (POST oms/v1/booking-details) =====

    public class TripjackBookingDetailsRequestWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;
    }

    public class TripjackBookingDetailsResponseWire
    {
        [JsonPropertyName("order")]
        public TripjackOrderWire? Order { get; set; }

        [JsonPropertyName("itemInfos")]
        public TripjackItemInfosWire? ItemInfos { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }
    }

    public class TripjackOrderWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        // SUCCESS (ticketed, paid) / ON_HOLD / CANCELLED / FAILED / PENDING (poll
        // again) / ABORTED / UNCONFIRMED (a hold that was released via unhold).
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class TripjackItemInfosWire
    {
        [JsonPropertyName("AIR")]
        public TripjackAirItemInfoWire? Air { get; set; }
    }

    public class TripjackAirItemInfoWire
    {
        [JsonPropertyName("tripInfos")]
        public List<TripjackTripOptionWire> TripInfos { get; set; } = new();

        [JsonPropertyName("travellerInfos")]
        public List<TripjackBookingTravellerInfoWire> TravellerInfos { get; set; } = new();
    }

    public class TripjackBookingTravellerInfoWire
    {
        // Keyed by "ORIGIN-DEST" (e.g. "DEL-BOM") -> the real Airline PNR for that
        // route. A oneway has exactly one entry; a roundtrip/multi-city would have
        // one per route (which may differ, same reasoning already applied to
        // Flyshop's own per-leg AirlinePnr — see TripBookingLeg.MarkTicketed).
        [JsonPropertyName("pnrDetails")]
        public Dictionary<string, string> PnrDetails { get; set; } = new();

        [JsonPropertyName("ti")]
        public string? Title { get; set; }

        [JsonPropertyName("pt")]
        public string? PaxType { get; set; }

        [JsonPropertyName("fN")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lN")]
        public string? LastName { get; set; }
    }

    // ===== Release PNR / Unhold (POST oms/v1/air/unhold) =====
    //
    // Requires BOTH bookingId and the PNR(s) to release — a bookingId alone is
    // rejected with errCode 1072 "Cancellation not available for PNR" (confirmed
    // live). The PNR(s) come from Booking Details' own travellerInfos[].pnrDetails.

    public class TripjackUnholdRequestWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;

        [JsonPropertyName("pnrs")]
        public List<string> Pnrs { get; set; } = new();
    }

    // Shared shape for any Tripjack response whose only interesting fields are the
    // status/error envelope (Unhold's own response has nothing else in it).
    public class TripjackStatusOnlyResponseWire
    {
        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    // ===== Cancellation — a three-call amendment flow, not a single call =====
    //
    // Prerequisite: the booking must be in Order Status SUCCESS (genuinely ticketed)
    // before any amendment can be raised — same constraint Flyshop has (a hold uses
    // Release PNR/unhold instead, never this).
    //
    //   1. POST oms/v1/air/amendment/amendment-charges (optional) — quotes the fee/
    //      refund without applying anything. { bookingId, type: "CANCELLATION",
    //      remarks, trips[]? } — omit trips[] to price the whole booking.
    //   2. POST oms/v1/air/amendment/submit-amendment — same request shape, commits
    //      the cancellation. Returns an amendmentId.
    //   3. POST oms/v1/air/amendment/amendment-details — { amendmentId }, poll (4-5x
    //      / 10s apart) until status is SUCCESS or REJECTED, not REQUESTED/PENDING.
    //
    // This has no Flyshop-shaped equivalent (single Air_TicketCancellation call with
    // a CancellationType/CancelCode) — IFlightSupplierClient.CancelBookingAsync's
    // single-call, fire-and-forget contract doesn't fit a charge-then-poll flow, so
    // implementing this for real will need that interface (or at least this method)
    // to change shape, not just a TripjackClient method body. Not yet implemented.
    //
    // Still entirely unverified against a live response (unlike everything above):
    //   Seat Map        POST fms/v1/seat                    { bookingId }
    //   Fare Rule       POST fms/v2/farerule                { flowType, id }
    //   Fare Validate   POST oms/v1/air/book/fare-validate  same shape as Book, no paymentInfos
    //
    // CreateBlockTicketAsync currently only implements the Hold half of Book (no
    // paymentInfos) — AddPaymentAsync/BookTicketAsync (Confirm-Book, real payment)
    // and CancelBookingAsync/GetFareRulesAsync/GetSeatMapAsync/GetAncillariesAsync
    // all still throw NotSupportedException in TripjackClient.
}
