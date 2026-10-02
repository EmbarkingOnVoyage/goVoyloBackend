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
        // Segment ID — this is what ssrBaggageInfos[].key/ssrMealInfos[].key/etc.
        // expect at Book time, and what GetAncillariesAsync's own SsrKey encodes.
        // Confirmed live it's present on both Search's and Review's segment shape.
        [JsonPropertyName("id")]
        public string? Id { get; set; }

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

        // Only populated on Review's own tripInfos (not Search's) — confirmed live.
        [JsonPropertyName("ssrInfo")]
        public TripjackSegmentSsrInfoWire? SsrInfo { get; set; }
    }

    public class TripjackSegmentSsrInfoWire
    {
        [JsonPropertyName("BAGGAGE")]
        public List<TripjackSsrOptionWire>? Baggage { get; set; }

        [JsonPropertyName("MEAL")]
        public List<TripjackSsrOptionWire>? Meal { get; set; }

        [JsonPropertyName("EXTRASERVICES")]
        public List<TripjackSsrOptionWire>? ExtraServices { get; set; }
    }

    public class TripjackSsrOptionWire
    {
        // Pass verbatim to travellerInfo[].ssrBaggageInfos[]/ssrMealInfos[]/
        // ssrExtraServiceInfos[].code at Book time.
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        // Absent for connecting-segment baggage (same baggage applies to the whole
        // journey — see this file's own POST-BOOKING SSR notes).
        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;
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

        // Special-return pairing: this fare's own id (sri), and the sri values of
        // the other leg's fares it may be combined with (msri). Only set on
        // pairable SPECIAL_RETURN fares — confirmed live on a domestic return
        // search: every onward msri matched a return sri and vice versa.
        [JsonPropertyName("sri")]
        public string? SpecialReturnId { get; set; }

        [JsonPropertyName("msri")]
        public List<string>? MatchingSpecialReturnIds { get; set; }

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

    // ===== Fare Rule (POST fms/v2/farerule) =====
    //
    // flowType SEARCH (id = priceId) and REVIEW (id = bookingId) both confirmed
    // live to return the same shape. GetFareRulesAsync is always called right after
    // a reprice (same pattern as GetAncillariesAsync/GetSeatMapAsync's own doc
    // comments), so this client always sends REVIEW with a bookingId.

    public class TripjackFareRuleRequestWire
    {
        [JsonPropertyName("flowType")]
        public string FlowType { get; set; } = "REVIEW";

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class TripjackFareRuleResponseWire
    {
        // Keyed by route ("DEP-ARR") — confirmed live. The docs' own field table
        // reads as if tfr sat flat at the response root; it's actually nested one
        // level deeper, per route.
        [JsonPropertyName("fareRule")]
        public Dictionary<string, TripjackRouteFareRuleWire>? FareRule { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    public class TripjackRouteFareRuleWire
    {
        // Cat 16 plain-text rule for when no structured mini-rule is available (per
        // the docs' own note) — always seen empty ({}) in the one live route checked
        // (a structured tfr was present instead), so its populated shape is
        // unverified. Left untyped since neither its presence nor its shape when
        // non-empty has been confirmed.
        [JsonPropertyName("fr")]
        public object? PlainTextRule { get; set; }

        // Keyed by policy type: CANCELLATION / DATECHANGE / NO_SHOW /
        // SEAT_CHARGEABLE — confirmed live, all four present for a real route.
        [JsonPropertyName("tfr")]
        public Dictionary<string, List<TripjackFareRulePolicyWire>>? TimedFareRule { get; set; }
    }

    public class TripjackFareRulePolicyWire
    {
        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("additionalFee")]
        public decimal? AdditionalFee { get; set; }

        // Confirmed live: these come back as STRINGS ("0", "4"), not integers the
        // way the docs' own field table describes them.
        [JsonPropertyName("st")]
        public string? StartTimeHours { get; set; }

        [JsonPropertyName("et")]
        public string? EndTimeHours { get; set; }

        // BEFORE_DEPARTURE / AFTER_DEPARTURE / DEFAULT — docs say the API returns
        // either st/et OR pp, never both; not seen live (every policy in the one
        // response checked used st/et).
        [JsonPropertyName("pp")]
        public string? PolicyPeriod { get; set; }

        [JsonPropertyName("policyInfo")]
        public string? PolicyInfo { get; set; }

        [JsonPropertyName("fcs")]
        public TripjackFareRuleChargesWire? Charges { get; set; }
    }

    public class TripjackFareRuleChargesWire
    {
        [JsonPropertyName("ARF")]
        public decimal? AirlineRescheduleFee { get; set; }

        [JsonPropertyName("ARFT")]
        public decimal? AirlineRescheduleFeeTax { get; set; }

        [JsonPropertyName("CRF")]
        public decimal? TripjackRescheduleFee { get; set; }

        [JsonPropertyName("CRFT")]
        public decimal? TripjackRescheduleFeeTax { get; set; }

        [JsonPropertyName("ACF")]
        public decimal? AirlineCancellationFee { get; set; }

        [JsonPropertyName("ACFT")]
        public decimal? AirlineCancellationFeeTax { get; set; }

        [JsonPropertyName("CCF")]
        public decimal? TripjackCancellationFee { get; set; }

        [JsonPropertyName("CCFT")]
        public decimal? TripjackCancellationFeeTax { get; set; }
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

        // A plain array here (unlike Search's tripInfos, which is a dictionary keyed
        // by ONWARD/RETURN/COMBO/index — see GetTripLegIndex's own doc comment) —
        // confirmed live. Array index is trusted as leg index since it's the natural
        // request/response order for the priceIds array Review was called with; not
        // independently re-verified the way Search's key-order assumption was (see
        // SearchAsync's own bug fix comment).
        [JsonPropertyName("tripInfos")]
        public List<TripjackTripOptionWire> TripInfos { get; set; } = new();
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

    // ===== Seat Map (POST fms/v1/seat) — only call when Review's own
    // conditions.isa was true. =====

    public class TripjackSeatMapRequestWire
    {
        [JsonPropertyName("bookingId")]
        public string BookingId { get; set; } = string.Empty;
    }

    public class TripjackSeatMapResponseWire
    {
        [JsonPropertyName("tripSeatMap")]
        public TripjackTripSeatMapWire? TripSeatMap { get; set; }

        [JsonPropertyName("status")]
        public TripjackStatusWire? Status { get; set; }

        [JsonPropertyName("errors")]
        public List<TripjackErrorWire>? Errors { get; set; }
    }

    public class TripjackTripSeatMapWire
    {
        // Nested one level deeper than the docs' own field table implies (it reads
        // "tripSeat[key], parent tripSeatMap" as if tripSeat were flat under
        // tripSeatMap) — confirmed live it's { tripSeatMap: { tripSeat: { ... } } }.
        // Keyed by segment ID, matching Review's own tripInfos[].sI[].id.
        [JsonPropertyName("tripSeat")]
        public Dictionary<string, TripjackSegmentSeatMapWire> TripSeat { get; set; } = new();
    }

    public class TripjackSegmentSeatMapWire
    {
        [JsonPropertyName("sData")]
        public TripjackSeatDeckWire? SeatDeck { get; set; }

        // Present (a reason string) when this leg genuinely has no seat map — not
        // seat data itself.
        [JsonPropertyName("nt")]
        public string? Note { get; set; }

        // A flat list, not pre-grouped into rows the way Flyshop's own
        // Air_GetSeatMap response is — GetSeatMapAsync groups by
        // seatPosition.row itself.
        [JsonPropertyName("sInfo")]
        public List<TripjackSeatWire>? Seats { get; set; }
    }

    public class TripjackSeatDeckWire
    {
        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }
    }

    public class TripjackSeatWire
    {
        [JsonPropertyName("seatNo")]
        public string SeatNo { get; set; } = string.Empty;

        [JsonPropertyName("seatPosition")]
        public TripjackSeatPositionWire SeatPosition { get; set; } = new();

        [JsonPropertyName("isBooked")]
        public bool IsBooked { get; set; }

        // Confirmed live: lowercase 'r' ("isLegroom"), not "isLegRoom" the way the
        // docs' own field table spells it.
        [JsonPropertyName("isLegroom")]
        public bool IsLegroom { get; set; }

        [JsonPropertyName("isAisle")]
        public bool IsAisle { get; set; }

        [JsonPropertyName("isWindow")]
        public bool IsWindow { get; set; }

        // In the docs' own field table but not seen in the one live response
        // checked (no exit-row seats on that particular route/aircraft) —
        // unverified whether the key name/casing is right.
        [JsonPropertyName("isExitRow")]
        public bool IsExitRow { get; set; }

        // Pass verbatim to travellerInfo[].ssrSeatInfos[].code at Book time.
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class TripjackSeatPositionWire
    {
        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }
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

        [JsonPropertyName("ssrBaggageInfos")]
        public List<TripjackSsrSelectionWire>? SsrBaggageInfos { get; set; }

        [JsonPropertyName("ssrMealInfos")]
        public List<TripjackSsrSelectionWire>? SsrMealInfos { get; set; }

        [JsonPropertyName("ssrSeatInfos")]
        public List<TripjackSsrSelectionWire>? SsrSeatInfos { get; set; }

        [JsonPropertyName("ssrExtraServiceInfos")]
        public List<TripjackSsrSelectionWire>? SsrExtraServiceInfos { get; set; }
    }

    public class TripjackSsrSelectionWire
    {
        // Segment ID from Review's own tripInfos[].sI[].id.
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        // SSR code from Review's own sI[].ssrInfo[category][].code.
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
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

        // Confirmed live: unlike deliveryInfo, Booking Details does NOT echo gstInfo
        // back — a Hold booked with a real gstInfo (verified against Tripjack's own
        // GSTIN validator) still comes back with no gstInfo key in `order` at all.
        // This field will therefore always be null in practice, so
        // BookTicketAsync's resend at Confirm-Book time is currently a guaranteed
        // no-op: whatever GST invoice was registered at Hold time either already
        // stuck server-side without needing a resend, or is silently lost — which of
        // the two is true is NOT verified (would need a live Confirm-Book run to
        // check the final ticket's own GST invoice, which automated testing here
        // can't trigger). If it turns out GST needs to survive to the final ticket
        // and isn't retained server-side, this needs GstInfo persisted on our own
        // TripBooking record instead and threaded into BookTicketAsync some other
        // way — bookingRefNo alone can't recover it.
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

        // Passport/PAN/document-id — confirmed live (international Book request with
        // pNum/eD/pNat/pid all present) that Booking Details DOES echo these back
        // under travellerInfos[], unlike gstInfo (see TripjackOrderWire.GstInfo's own
        // doc comment) — so BookTicketAsync's resend of these at Confirm-Book time is
        // sound. PAN (pan) and document-id (di) specifically weren't exercised in
        // that same test (no PAN/student-senior fare test case run), but share the
        // same wire shape as the confirmed fields.
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

    // Live-verification status as of this pass (see each method's own doc comment
    // in TripjackClient for specifics):
    //   Confirmed live: Search/Review/Book(Hold)/Booking-Details/Unhold (earlier
    //     pass), multi-priceId Review batching (domestic return, one bookingId,
    //     correctly summed combined fare), Confirm-Fare-Before-Ticket
    //     (oms/v1/air/fare-validate), GST field acceptance at Book time (against
    //     Tripjack's own GSTIN validator), passport field acceptance AND echo-back
    //     through Booking Details (international Book + Booking Details).
    //   Confirmed live as a real gap: Booking Details does NOT echo gstInfo back
    //     the way it does deliveryInfo/travellerInfo — see TripjackOrderWire's own
    //     doc comment. BookTicketAsync's GST resend at Confirm-Book time is
    //     currently always a no-op as a result.
    //   GetAncillariesAsync is implemented (Review's own ssrInfo, cached by
    //     bookingId since Tripjack has no way to re-fetch it later — see
    //     TripjackClient's own SsrCacheTtl doc comment) and SSR selections are
    //     threaded into Book's travellerInfo — see TripjackClient.MapTraveller and
    //     MapSsrOptions. Confirmed live end to end: a real Book with a baggage (BOF1,
    //     100.0) and meal (VGML, 0.0) selection came back with matching BP/MP amounts
    //     in Booking Details' own fare breakdown (fd.fC.BP/MP), proving the
    //     selections weren't just accepted but actually priced in.
    //   GetSeatMapAsync is implemented (POST fms/v1/seat) and seat selections thread
    //     into Book's travellerInfo the same way SSR selections do — see
    //     TripjackClient.MapSeat/GetSeatMapAsync. Confirmed live end to end: a real
    //     Book with a seat (1A, 999.0) selection came back with a matching SP amount
    //     in Booking Details' own fare breakdown. Two things confirmed live that
    //     differ from the docs' own field table: the response is nested one level
    //     deeper (tripSeatMap.tripSeat, not tripSeatMap directly), and the legroom
    //     field is "isLegroom" (lowercase r), not "isLegRoom". isExitRow itself
    //     wasn't seen in the one response checked (no exit-row seats on that
    //     route/aircraft) — unverified whether that field name/casing is right.
    //   GetFareRulesAsync is implemented (POST fms/v2/farerule, flowType REVIEW) and
    //     flattens Tripjack's structured per-policy-type time bands into the same
    //     free-text FareRuleDesc shape Flyshop's own fare rules use — see
    //     TripjackClient.FormatFareRulePolicies. The real live response shape
    //     differs from the docs' own field table in two ways: tfr is nested under
    //     fareRule.{route} (keyed by "DEP-ARR"), not flat at the response root, and
    //     st/et come back as strings ("0", "4"), not integers. Structurally
    //     confirmed against a real response (all four policy types — CANCELLATION/
    //     DATECHANGE/NO_SHOW/SEAT_CHARGEABLE — present and correctly shaped) but not
    //     run through this app's own endpoint end to end the way SSR/seat map were,
    //     since fare rules aren't applied to a booking the way a price is, so there's
    //     no equivalent "did it actually take effect" check available.
    //   NOT live-verified: AddPaymentAsync/BookTicketAsync's own Confirm-Book call,
    //     and CancelBookingAsync's amendment flow — both commit a real
    //     payment+ticketing or cancellation+refund even on the UAT sandbox, which
    //     automated testing in this environment isn't allowed to trigger.
    //   Still entirely unverified against a live response (docs-only, no field
    //   table given):
    //     Fare Validate (Instant)  POST oms/v1/air/book/fare-validate  same shape as Book, no paymentInfos
    // Every IFlightSupplierClient method is now implemented for Tripjack.
}
