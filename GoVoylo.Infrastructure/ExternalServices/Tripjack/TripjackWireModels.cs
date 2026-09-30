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
        public TripjackTotalFareDetailWire? TotalFareDetail { get; set; }
    }

    // Unlike the per-pax fd[paxType].fC shape, totalFareDetail wraps its fare
    // component one level deeper than it looks from the field table alone —
    // confirmed live: totalPriceInfo.totalFareDetail.fC.{BF,TF,TAF}, not
    // totalPriceInfo.totalFareDetail.{BF,TF,TAF} directly.
    public class TripjackTotalFareDetailWire
    {
        [JsonPropertyName("fC")]
        public TripjackFareComponentWire FareComponent { get; set; } = new();
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

        // Required whenever Review's own conditions.iecr was true (mandatory for
        // certain international fares) — sent unconditionally, populated from the
        // same contact details as deliveryInfo, since nothing upstream of this client
        // collects a genuinely separate "emergency contact" today. Harmless to send
        // when iecr is false; Tripjack docs don't say it's rejected in that case.
        [JsonPropertyName("contactInfo")]
        public TripjackContactInfoWire? ContactInfo { get; set; }

        [JsonPropertyName("travellerInfo")]
        public List<TripjackTravellerInfoWire> TravellerInfo { get; set; } = new();

        // Omit entirely for Hold. Present (with Amount = Review's Total Fare) for
        // Instant Book or Confirm-Book — Tripjack determines the payment medium
        // itself; nothing else about how the agent pays goes in this request.
        [JsonPropertyName("paymentInfos")]
        public List<TripjackPaymentInfoWire>? PaymentInfos { get; set; }

        // Required whenever Review's own conditions.igm was true, optional (but
        // still accepted) whenever conditions.gstappl was true.
        [JsonPropertyName("gstInfo")]
        public TripjackGstInfoWire? GstInfo { get; set; }
    }

    public class TripjackContactInfoWire
    {
        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; } = new();

        [JsonPropertyName("contacts")]
        public List<string> Contacts { get; set; } = new();

        // Emergency contact name.
        [JsonPropertyName("ecn")]
        public string Ecn { get; set; } = string.Empty;
    }

    public class TripjackGstInfoWire
    {
        // 15-digit GSTIN.
        [JsonPropertyName("gstNumber")]
        public string GstNumber { get; set; } = string.Empty;

        // Max 35 chars, IATA standard.
        [JsonPropertyName("registeredName")]
        public string RegisteredName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }

        // Max 70 chars, IATA standard.
        [JsonPropertyName("address")]
        public string? Address { get; set; }
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

        // Required when Review's own conditions.pm was true (passport-mandatory
        // fares, typically international).
        [JsonPropertyName("pNum")]
        public string? PassportNumber { get; set; }

        // YYYY-MM-DD. Required when conditions.pped was true — Tripjack itself
        // additionally rejects a passport expiring within 6 months of travel
        // (errCode 1067).
        [JsonPropertyName("eD")]
        public string? PassportExpiry { get; set; }

        // 2-letter IATA country code.
        [JsonPropertyName("pNat")]
        public string? PassportNationality { get; set; }

        // YYYY-MM-DD.
        [JsonPropertyName("pid")]
        public string? PassportIssueDate { get; set; }

        // Required when conditions.ipa was true.
        [JsonPropertyName("pan")]
        public string? PanNumber { get; set; }

        // Student/senior-citizen fares — required when conditions.idm was true.
        [JsonPropertyName("di")]
        public string? DocumentId { get; set; }
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

        // Confirmed live: Booking Details echoes back the same emails/contacts Book
        // was originally called with — Confirm-Book needs this resent (it shares
        // Book's request shape), and this is the only place it's available again
        // from just a bookingId, so BookTicketAsync reads it from here rather than
        // needing PassengerEmail/Mobile threaded through IFlightSupplierClient's
        // bookingRefNo-only signature.
        [JsonPropertyName("deliveryInfo")]
        public TripjackDeliveryInfoWire? DeliveryInfo { get; set; }

        // NOT live-verified whether Booking Details actually echoes gstInfo back the
        // way it does deliveryInfo (Tripjack's UAT sandbox started rate-limiting
        // this account mid-session before this could be confirmed) — assumed
        // consistent with the deliveryInfo behavior above. If it turns out absent,
        // BookTicketAsync's own resend of GstInfo at Confirm-Book time silently
        // becomes a no-op (null-propagated), not a hard failure.
        [JsonPropertyName("gstInfo")]
        public TripjackGstInfoWire? GstInfo { get; set; }

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

        // YYYY-MM-DD — only present when it was submitted at Book time (mandatory
        // for INFANT, optional otherwise). Resent to Confirm-Book verbatim rather
        // than re-derived, same reasoning as DeliveryInfo above.
        [JsonPropertyName("dob")]
        public string? DateOfBirth { get; set; }

        // Passport/PAN/document-id — same "echoed back from Book, resent verbatim at
        // Confirm-Book" reasoning as DateOfBirth above. NOT live-verified (see
        // TripjackOrderWire.GstInfo's own doc comment for why).
        [JsonPropertyName("pNum")]
        public string? PassportNumber { get; set; }

        [JsonPropertyName("eD")]
        public string? PassportExpiry { get; set; }

        [JsonPropertyName("pNat")]
        public string? PassportNationality { get; set; }

        [JsonPropertyName("pid")]
        public string? PassportIssueDate { get; set; }

        [JsonPropertyName("pan")]
        public string? PanNumber { get; set; }

        [JsonPropertyName("di")]
        public string? DocumentId { get; set; }
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
    // Doesn't need IFlightSupplierClient.CancelBookingAsync's contract to change
    // shape after all — the 3 calls run sequentially inside one method body, the
    // same "await a delay, then poll" pattern already used for Book/Confirm-Book,
    // just longer (up to ~40-50s worst case across 4-5 polls). Only full-booking
    // cancellation is supported (trips[]/travellers[] scoping is omitted) since
    // Tripjack addresses a partial cancel by src/dest/departureDate/traveller name,
    // not by the Flyshop-shaped Flight_Id/Segment_Id our own
    // SupplierCancellationRequestDto.Segments carries — acceptable today since
    // Tripjack bookings are oneway-only anyway (see CreateTempBookingAsync's own
    // guard), so "cancel this booking's one leg" and "cancel the whole booking" are
    // the same operation.

    public class TripjackAmendmentRequestWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;

        // CANCELLATION / VOIDED / FULL_REFUND.
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("remarks")]
        public string Remarks { get; set; } = string.Empty;
    }

    public class TripjackAmendmentChargesResponseWire
    {
        [JsonPropertyName("amendmentCharges")]
        public decimal AmendmentCharges { get; set; }

        [JsonPropertyName("refundableAmount")]
        public decimal RefundableAmount { get; set; }

        [JsonPropertyName("totalFare")]
        public decimal TotalFare { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    public class TripjackSubmitAmendmentResponseWire
    {
        [JsonPropertyName("bookingId")]
        public string? BookingId { get; set; }

        [JsonPropertyName("amendmentId")]
        public string? AmendmentId { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    public class TripjackAmendmentDetailsRequestWire
    {
        [JsonPropertyName("amendmentId")]
        public string AmendmentId { get; set; } = string.Empty;
    }

    public class TripjackAmendmentDetailsResponseWire
    {
        [JsonPropertyName("bookingId")]
        public string? BookingId { get; set; }

        [JsonPropertyName("amendmentId")]
        public string? AmendmentId { get; set; }

        // REQUESTED (still processing — keep polling) / SUCCESS / REJECTED / PENDING.
        [JsonPropertyName("amendmentStatus")]
        public string AmendmentStatus { get; set; } = string.Empty;

        [JsonPropertyName("refundableAmount")]
        public decimal RefundableAmount { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    // AddPaymentAsync/BookTicketAsync (Confirm-Book) and CancelBookingAsync (the
    // amendment flow) are all implemented against Tripjack's documented contract —
    // none of the three were live-verified end to end, since each commits a real
    // payment, ticketing, or cancellation+refund even on the UAT sandbox, which
    // automated testing in this environment isn't allowed to trigger. See each
    // method's own doc comment in TripjackClient for specifics. Still entirely
    // unverified against a live response (docs-only, no field table given):
    //   Seat Map        POST fms/v1/seat                    { bookingId }
    //   Fare Rule       POST fms/v2/farerule                { flowType, id }
    //   Fare Validate (Instant)  POST oms/v1/air/book/fare-validate  same shape as Book, no paymentInfos
    // GetFareRulesAsync/GetSeatMapAsync/GetAncillariesAsync still throw
    // NotSupportedException in TripjackClient. Multi-leg (roundtrip/multi-city)
    // itineraries remain unsupported — see CreateTempBookingAsync's own guard.
}
