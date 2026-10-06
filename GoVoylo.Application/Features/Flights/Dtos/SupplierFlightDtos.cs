namespace GoVoylo.Application.Features.Flights.Dtos
{
    public record SupplierFlightSegmentDto(
        string Origin,
        string Destination,
        string AirlineCode,
        string AirlineName,
        string FlightNumber,
        DateTime DepartureDateTime,
        DateTime ArrivalDateTime,
        string Duration,
        // Which trip of a whole-trip offer this segment belongs to (Tripjack COMBO:
        // 0 outbound, 1 return, ...). Always 0 for a single-trip offer.
        int TripIndex = 0);

    // TotalAmount is per adult (the fare picker's "/adult" figure);
    // BookingTotalAmount covers every searched passenger. FareIdentifier,
    // SpecialReturnId and MatchingSpecialReturnIds describe supplier-defined
    // special-return pairing (Tripjack's fareIdentifier/sri/msri): a fare is only
    // bookable on a round trip with a fare on the other leg whose SpecialReturnId
    // is in this fare's MatchingSpecialReturnIds.
    public record SupplierFareOptionDto(
        string FareId,
        bool Refundable,
        decimal TotalAmount,
        string CurrencyCode,
        string? CheckInBaggage,
        string? HandBaggage,
        decimal BookingTotalAmount = 0m,
        string? FareIdentifier = null,
        string? SpecialReturnId = null,
        IReadOnlyList<string>? MatchingSpecialReturnIds = null,
        // Base fare (before taxes and fees) of BookingTotalAmount — every searched
        // passenger. The rest of BookingTotalAmount is taxes and fees.
        decimal BookingBaseAmount = 0m);

    public record SupplierFlightOptionDto(
        string FlightKey,
        string FareId,
        string AirlineCode,
        string AirlineName,
        bool Refundable,
        bool IsLowCostCarrier,
        IReadOnlyList<SupplierFlightSegmentDto> Segments,
        decimal TotalAmount,
        string CurrencyCode,
        int SeatsAvailable,
        IReadOnlyList<SupplierFareOptionDto> Fares,
        // Index into the search request's Segments list this option satisfies —
        // 0 for a one-way/onward leg, 1 for a round-trip return leg, 0..N-1 for
        // multi-city. Mirrors the supplier's own Trip_Id grouping so the app can
        // tell which leg each offer belongs to instead of getting one flat list.
        int TripLegIndex = 0);

    public record SupplierFlightSearchResultDto(
        string SearchKey,
        IReadOnlyList<SupplierFlightOptionDto> Flights);

    public record SupplierRepriceRequestDto(string SearchKey, string FlightKey, string FareId);

    public record SupplierRepriceResultDto(
        string FlightKey,
        string FareId,
        decimal TotalAmount,
        string CurrencyCode,
        bool Repriced,
        bool IsFareChange);

    // Origin/Destination/TravelDate/AirlineCode/AirlineName/FlightNumber/TotalAmount/
    // CurrencyCode are captured once at search time (from the first/last segment for
    // a connecting flight) purely so CreateBookingCommandHandler can persist a
    // TripBooking without re-deriving route/price data it no longer has access to
    // after Reprice/TempBooking — they're never used for pricing or supplier calls,
    // only for the "My Trips" record. Reprice's `with` updates only touch
    // FlightKey/FareId, so these fields stay exactly as captured at search time.
    public record FlightOfferSession(
        string SupplierCode,
        string SearchKey,
        string FlightKey,
        string FareId,
        string Origin,
        string Destination,
        DateTime TravelDate,
        string AirlineCode,
        string AirlineName,
        string FlightNumber,
        decimal TotalAmount,
        string CurrencyCode,
        // Set only for a whole-trip offer (e.g. a Tripjack international return):
        // one entry per trip, so the booking is saved as DEL-DXB + DXB-DEL rather
        // than one DEL-DEL leg.
        IReadOnlyList<FlightOfferSessionTrip>? Trips = null);

    public record FlightOfferSessionTrip(
        string Origin,
        string Destination,
        DateTime TravelDate,
        string AirlineCode,
        string AirlineName,
        string FlightNumber);

    public record SupplierLowFareRequestDto(string Origin, string Destination, int Month, int Year);

    public record SupplierLowFareDayDto(
        DateTime TravelDate,
        decimal Amount,
        string CurrencyCode,
        string AirlineCode,
        string AirlineName);

    public record SupplierLowFareResultDto(IReadOnlyList<SupplierLowFareDayDto> Days);

    public record SupplierAncillaryRequestDto(string SearchKey, string FlightKey);

    // One priced SSR option — shared shape for a baggage/meal choice (from
    // Air_GetSSR) and a single seat (from Air_GetSeatMap, where SsrType is always
    // the SEAT type and SsrTypeName/SsrTypeDesc carry the seat label, e.g. "1A").
    public record SupplierAncillaryOptionDto(
        int SsrType,
        string SsrTypeName,
        string SsrTypeDesc,
        string? SsrCode,
        string SsrKey,
        // 0-ISLE/1-AVAILABLE/2-BLOCKED/3-BOOKED — meaningful for seats (from
        // Air_GetSeatMap); the docs say it's meaningless for Air_GetSSR baggage/meal
        // options, which always come back as 0.
        int SsrStatus,
        int LegIndex,
        int SegmentId,
        bool SegmentWise,
        decimal TotalAmount,
        string CurrencyCode,
        IReadOnlyList<int> ApplicablePaxTypes,
        // Seats only, when the supplier reports them (Tripjack does; Flyshop sends
        // its rows pre-laid-out with aisle placeholders instead): the seat's
        // row/column on the cabin grid — a skipped column is an aisle — and
        // extra-legroom / emergency-exit-row flags.
        int? SeatRow = null,
        int? SeatColumn = null,
        bool IsExtraLegroom = false,
        bool IsExitRow = false);

    public record SupplierAncillaryResultDto(IReadOnlyList<SupplierAncillaryOptionDto> Options);

    // PaxType: 0-ADT/1-CHD/2-INF, Gender: 0-Male/1-Female — mirrors the numeric
    // codes Flyshop's own PAX_Details/FareDetails already use elsewhere.
    public record SupplierPaxDetailDto(
        int PaxId, int PaxType, string Title, string FirstName, string LastName, int Gender);

    public record SupplierSeatMapRequestDto(
        string SearchKey, string FlightKey, IReadOnlyList<SupplierPaxDetailDto> Travelers);

    public record SupplierSeatRowDto(IReadOnlyList<SupplierAncillaryOptionDto> Seats);

    // Origin/Destination: the flight segment this map is for (a connecting leg
    // has one map per segment), when the supplier lets us tell.
    public record SupplierSeatSegmentDto(
        int LegIndex,
        IReadOnlyList<SupplierSeatRowDto> Rows,
        string? Origin = null,
        string? Destination = null);

    public record SupplierSeatMapResultDto(IReadOnlyList<SupplierSeatSegmentDto> Segments);

    // Passport/PAN/document-id fields — see BookingTravelerRequestDto's own doc
    // comment for why PassportNationality covers both "nationality" and "issuing
    // country".
    public record SupplierTempBookingPaxDto(
        int PaxId,
        int PaxType,
        string Title,
        string FirstName,
        string LastName,
        int Gender,
        DateTime? DateOfBirth = null,
        string? PassportNumber = null,
        string? PassportNationality = null,
        DateTime? PassportExpiry = null,
        DateTime? PassportIssueDate = null,
        string? PanNumber = null,
        string? DocumentId = null);

    public record SupplierBookingSsrDto(int PaxId, string SsrKey);

    public record SupplierBookingFlightDto(
        string SearchKey, string FlightKey, IReadOnlyList<SupplierBookingSsrDto> SelectedSsrs);

    public record SupplierTempBookingRequestDto(
        string PassengerMobile,
        string PassengerEmail,
        IReadOnlyList<SupplierTempBookingPaxDto> Travelers,
        IReadOnlyList<SupplierBookingFlightDto> Flights,
        bool Gst = false,
        string GstNumber = "",
        string GstHolderName = "",
        string GstAddress = "");

    // DeferredBookPayload is set only when the supplier couldn't hold this fare and
    // nothing was booked yet (see TripBooking.DeferredSupplierPayloadEncrypted) — the
    // caller must persist it and hand it back to AddPaymentAsync/BookTicketAsync.
    public record SupplierTempBookingResultDto(string BookingRefNo, string? DeferredBookPayload = null);

    // One AirlinePNRDetails entry from Air_Ticketing's response — for a multi-leg
    // (roundtrip/multi-city) booking there's one of these per flight, each with its
    // own Flight_Id (needed to cancel/release that specific leg later).
    public record SupplierTicketingLegResultDto(
        string FlightId,
        string StatusId,
        string? AirlineCode,
        string? AirlinePnr,
        string? CrsPnr,
        string? RecordLocator,
        string? FailureRemark);

    public record SupplierTicketingResultDto(
        string BookingRefNo,
        // First leg's values, kept for existing single-leg callers. 11-Success,
        // 22-Failed, 33-Block — see Air_Ticketing's own docs.
        string StatusId,
        string? AirlineCode,
        string? AirlinePnr,
        string? CrsPnr,
        string? RecordLocator,
        string? FailureRemark,
        IReadOnlyList<SupplierTicketingLegResultDto> Legs,
        // What the supplier will actually charge for this booking (all passengers,
        // plus selected SSRs), when it confirms one at hold time — fares can move
        // between search and booking (confirmed live: an infant fare dropped from
        // 6088.5 at Search to 3088.5 at Review). Null when the supplier doesn't
        // confirm a whole-booking amount here (Flyshop, for now).
        decimal? ConfirmedTotalAmount = null);

    public record SupplierFareRuleRequestDto(string SearchKey, string FlightKey, string FareId);

    // Plain text, HTML already stripped — Flyshop returns FareRuleDesc as a full
    // XHTML document (often just one boilerplate paragraph), not structured data.
    public record SupplierFareRuleDto(string SegmentId, string FareRuleName, string FareRuleDesc);

    // One time band of a structured fare rule (Tripjack): applies from StartHours
    // to EndHours before departure. Type: "Cancellation" / "DateChange" / "NoShow".
    // AirlineFee is per passenger; null when the airline sets it at the time
    // (shown as "Airline policy"). TransactionFee is the supplier's own fee on top.
    public record SupplierFareRulePolicyDto(
        string Route,
        string Type,
        int? StartHours,
        int? EndHours,
        decimal? AirlineFee,
        decimal? TransactionFee,
        string? Info);

    // Policies: structured bands when the supplier gives them (Tripjack); empty
    // for free-text-only suppliers (Flyshop), whose Rules carry the text.
    public record SupplierFareRuleResultDto(
        IReadOnlyList<SupplierFareRuleDto> Rules,
        IReadOnlyList<SupplierFareRulePolicyDto>? Policies = null);

    public record SupplierCancelSegmentDto(string FlightId, string PassengerId, string SegmentId);

    // ===== Post-booking: full booking details and cancellation quote (My Trips) =====

    // TripIndex: which trip of the booking the flight belongs to (0 outbound, 1 return, ...).
    // The airport fields are optional — what the supplier echoes back (Tripjack
    // Booking Details does; Flyshop's Air_Reprint only gives "CITY (CODE)").
    public record SupplierBookingSegmentDto(
        int TripIndex,
        string Origin,
        string Destination,
        string AirlineCode,
        string AirlineName,
        string FlightNumber,
        DateTime DepartureDateTime,
        DateTime ArrivalDateTime,
        int DurationMinutes,
        SupplierAirportDto? OriginAirport = null,
        SupplierAirportDto? DestinationAirport = null,
        int Stops = 0);

    public record SupplierAirportDto(string? Name, string? City, string? Terminal, string? CountryCode);

    // One passenger's ticket on one route ("DEL-BOM"): PNR, e-ticket number and
    // any paid seat / meal / extra baggage booked on it.
    public record SupplierPassengerTicketDto(
        string Route,
        string? Pnr,
        string? TicketNumber,
        string? Seat,
        string? Meal,
        string? ExtraBaggage);

    // PaxType: "Adult" / "Child" / "Infant". The rest is optional, filled where
    // the supplier returns it.
    public record SupplierBookingPassengerDto(
        string Title,
        string FirstName,
        string LastName,
        string PaxType,
        IReadOnlyList<SupplierPassengerTicketDto>? Tickets = null,
        string? CabinClass = null,
        string? CheckInBaggage = null,
        string? CabinBaggage = null);

    // A supplier's own cancellation rule for one passenger type (Flyshop's
    // Air_Reprint CancellationCharges) — used to estimate a refund when the supplier
    // can't quote one before cancelling.
    public record SupplierCancellationRuleDto(string PaxType, decimal Value, bool IsPercentage);

    // Fare amounts are for the whole booking (every passenger), as the supplier
    // reports them; null when the supplier doesn't break them down.
    public record SupplierBookingDetailsDto(
        IReadOnlyList<SupplierBookingSegmentDto> Segments,
        IReadOnlyList<SupplierBookingPassengerDto> Passengers,
        decimal? BaseFare,
        decimal? TaxesAndFees,
        decimal? TotalFare,
        string CurrencyCode,
        IReadOnlyList<SupplierCancellationRuleDto> CancellationRules,
        // The booking's contact details as the supplier holds them.
        string? ContactEmail = null,
        string? ContactPhone = null);

    // Origin/Destination/DepartureDate narrow the quote to one trip of the booking
    // (leg-only cancellation); all null means the whole booking.
    public record SupplierCancellationQuoteRequestDto(
        string BookingRefNo,
        string? AirlinePnr,
        string? Origin,
        string? Destination,
        DateTime? DepartureDate);

    // IsEstimate: the supplier couldn't quote before cancelling, so the charges are
    // estimated from its fare rules and the final amount is confirmed afterwards.
    public record SupplierCancellationQuoteDto(
        decimal TotalFare,
        decimal CancellationCharges,
        decimal RefundAmount,
        bool IsEstimate);

    // RefundAmount: what the supplier will refund, when it reports it; null if unknown.
    public record SupplierCancellationResultDto(decimal? RefundAmount);

    // CancellationType: 0-Normal Cancel (online, auto-cancelled with the airline's
    // applicable penalty) / 1-Full Refund (offline, only if a full refund applies) /
    // 2-No Show (offline). CancelCode is one of a fixed set of reason codes specific
    // to each CancellationType — see Air_TicketCancellation's own docs for the table;
    // not re-validated here since a wrong combination is something Flyshop's own API
    // already rejects.
    public record SupplierCancellationRequestDto(
        string RefNo,
        string AirlinePnr,
        int CancellationType,
        string CancelCode,
        string ReqRemarks,
        IReadOnlyList<SupplierCancelSegmentDto> Segments);

    public record SupplierReleaseHoldRequestDto(string BookingRefNo, string AirlinePnr);

    // Result of AddPayment — debiting GoVoylo's Flyshop agency wallet against a held
    // booking. StatusId here is AddPayment's own Response_Header.Status_Id (Flyshop
    // reuses the same 11-Success/22-Failed convention as Air_Ticketing).
    public record SupplierPaymentResultDto(decimal Amount, string? PaymentId, string StatusId);
}
