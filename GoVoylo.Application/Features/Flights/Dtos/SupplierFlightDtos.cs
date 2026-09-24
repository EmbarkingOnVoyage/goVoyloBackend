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
        string Duration);

    public record SupplierFareOptionDto(
        string FareId,
        bool Refundable,
        decimal TotalAmount,
        string CurrencyCode,
        string? CheckInBaggage,
        string? HandBaggage);

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
        string CurrencyCode);

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
        IReadOnlyList<int> ApplicablePaxTypes);

    public record SupplierAncillaryResultDto(IReadOnlyList<SupplierAncillaryOptionDto> Options);

    // PaxType: 0-ADT/1-CHD/2-INF, Gender: 0-Male/1-Female — mirrors the numeric
    // codes Flyshop's own PAX_Details/FareDetails already use elsewhere.
    public record SupplierPaxDetailDto(
        int PaxId, int PaxType, string Title, string FirstName, string LastName, int Gender);

    public record SupplierSeatMapRequestDto(
        string SearchKey, string FlightKey, IReadOnlyList<SupplierPaxDetailDto> Travelers);

    public record SupplierSeatRowDto(IReadOnlyList<SupplierAncillaryOptionDto> Seats);

    public record SupplierSeatSegmentDto(int LegIndex, IReadOnlyList<SupplierSeatRowDto> Rows);

    public record SupplierSeatMapResultDto(IReadOnlyList<SupplierSeatSegmentDto> Segments);

    public record SupplierTempBookingPaxDto(
        int PaxId, int PaxType, string Title, string FirstName, string LastName, int Gender);

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

    public record SupplierTempBookingResultDto(string BookingRefNo);

    // One AirlinePNRDetails entry from Air_Ticketing's response — for a multi-leg
    // (roundtrip/multi-city) booking there's one of these per flight, each with its
    // own Flight_Id (needed to cancel/release that specific leg later).
    public record SupplierTicketingLegResultDto(
        string FlightId,
        string StatusId,
        string? AirlineCode,
        string? AirlinePnr,
        string? RecordLocator,
        string? FailureRemark);

    public record SupplierTicketingResultDto(
        string BookingRefNo,
        // First leg's values, kept for existing single-leg callers. 11-Success,
        // 22-Failed, 33-Block — see Air_Ticketing's own docs.
        string StatusId,
        string? AirlineCode,
        string? AirlinePnr,
        string? RecordLocator,
        string? FailureRemark,
        IReadOnlyList<SupplierTicketingLegResultDto> Legs);

    public record SupplierFareRuleRequestDto(string SearchKey, string FlightKey, string FareId);

    // Plain text, HTML already stripped — Flyshop returns FareRuleDesc as a full
    // XHTML document (often just one boilerplate paragraph), not structured data.
    public record SupplierFareRuleDto(string SegmentId, string FareRuleName, string FareRuleDesc);

    public record SupplierFareRuleResultDto(IReadOnlyList<SupplierFareRuleDto> Rules);

    public record SupplierCancelSegmentDto(string FlightId, string PassengerId, string SegmentId);

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
}
