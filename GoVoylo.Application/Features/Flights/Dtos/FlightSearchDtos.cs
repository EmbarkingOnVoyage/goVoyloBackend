namespace GoVoylo.Application.Features.Flights.Dtos
{
    public record FlightSearchSegmentDto(string Origin, string Destination, DateTime TravelDate);

    public record FlightSearchRequestDto(
        string TripType,
        string CabinClass,
        IReadOnlyList<FlightSearchSegmentDto> Segments,
        int AdultCount,
        int ChildCount,
        int InfantCount);

    public record FlightOfferSegmentDto(
        string Origin,
        string Destination,
        string AirlineCode,
        string AirlineName,
        string FlightNumber,
        DateTime DepartureDateTime,
        DateTime ArrivalDateTime,
        string Duration);

    public record FareOptionDto(
        string FareId,
        bool Refundable,
        decimal TotalAmount,
        string CurrencyCode,
        string? CheckInBaggage,
        string? HandBaggage);

    public record FlightOfferDto(
        Guid OfferId,
        string AirlineCode,
        string AirlineName,
        bool Refundable,
        bool IsLowCostCarrier,
        IReadOnlyList<FlightOfferSegmentDto> Segments,
        decimal TotalAmount,
        string CurrencyCode,
        int SeatsAvailable,
        IReadOnlyList<FareOptionDto> Fares,
        // See SupplierFlightOptionDto.TripLegIndex — passed through unchanged so
        // the client can split a round-trip/multi-city response by leg.
        int TripLegIndex = 0);

    public record FlightSearchResponseDto(IReadOnlyList<FlightOfferDto> Offers);

    public record FlightRepriceResponseDto(
        Guid OfferId,
        decimal TotalAmount,
        string CurrencyCode,
        bool PriceChanged);

    public record FareCalendarDayDto(DateTime Date, decimal Amount, string CurrencyCode);

    public record FareCalendarResponseDto(IReadOnlyList<FareCalendarDayDto> Days);

    public record AncillaryOptionDto(
        int SsrType,
        string SsrTypeName,
        string SsrTypeDesc,
        string? SsrCode,
        string SsrKey,
        int SsrStatus,
        int LegIndex,
        int SegmentId,
        bool SegmentWise,
        decimal TotalAmount,
        string CurrencyCode,
        IReadOnlyList<int> ApplicablePaxTypes);

    public record FlightAncillariesResponseDto(IReadOnlyList<AncillaryOptionDto> Options);

    // PaxType/Gender as plain strings at the API boundary ("Adult"/"Child"/"Infant",
    // "Male"/"Female") so callers don't need to know Flyshop's numeric codes — the
    // handler maps them internally.
    public record SeatMapTravelerRequestDto(
        string Title, string FirstName, string LastName, string Gender, string PaxType);

    public record SeatMapRowDto(IReadOnlyList<AncillaryOptionDto> Seats);

    public record SeatMapSegmentDto(int LegIndex, IReadOnlyList<SeatMapRowDto> Rows);

    public record SeatMapResponseDto(IReadOnlyList<SeatMapSegmentDto> Segments);

    public record BookingSsrSelectionDto(int PaxId, string SsrKey);

    public record BookingLegRequestDto(Guid OfferId, IReadOnlyList<BookingSsrSelectionDto> SelectedSsrs);

    // PaxType/Gender as plain strings at the API boundary, same reasoning as
    // SeatMapTravelerRequestDto — the handler maps them to Flyshop's numeric codes.
    // DateOfBirth is optional for adults/children but required by Flyshop for
    // infants (Air_Reprice's own Required_PAX_Details marks DOB mandatory only for
    // Pax_Type 2-INF) — omitting it for an infant traveler will fail Air_TempBooking.
    public record BookingTravelerRequestDto(
        int PaxId,
        string Title,
        string FirstName,
        string LastName,
        string Gender,
        string PaxType,
        DateTime? DateOfBirth = null);

    // GST fields are optional — omit all three for a booking with no GST invoice.
    // Air_TempBooking's own GST flag is derived from whether GstNumber is present,
    // not passed separately, so there's no way to send an inconsistent combination.
    public record CreateBookingRequestDto(
        IReadOnlyList<BookingLegRequestDto> Legs,
        IReadOnlyList<BookingTravelerRequestDto> Travelers,
        string PassengerMobile,
        string PassengerEmail,
        string? GstNumber = null,
        string? GstHolderName = null,
        string? GstAddress = null);

    public record CreateBookingResponseDto(
        string BookingRefNo,
        // 11-Success, 22-Failed, 33-Block — see Air_Ticketing's own docs.
        string StatusId,
        string? AirlineCode,
        string? AirlinePnr,
        string? RecordLocator,
        string? FailureRemark);

    public record FareRuleDto(string SegmentId, string FareRuleName, string FareRuleDesc);

    public record LegFareRulesDto(Guid OfferId, IReadOnlyList<FareRuleDto> Rules);

    public record FareRulesResponseDto(IReadOnlyList<LegFareRulesDto> Legs);

    public record CancelBookingSegmentDto(string FlightId, string PassengerId, string SegmentId);

    public record CancelBookingResponseDto(bool Success);

    public record ReleaseHoldResponseDto(bool Success);

    public record TripBookingLegDto(
        int LegIndex,
        string Origin,
        string Destination,
        DateTime TravelDate,
        string AirlineCode,
        string AirlineName,
        string FlightNumber);

    public record TripBookingDto(
        Guid Id,
        string BookingRefNo,
        string? AirlinePnr,
        // Flyshop's status at creation time (11-Success/22-Failed/33-Block).
        string StatusId,
        // App-tracked lifecycle: Active/Cancelled/Released — see TripBooking.
        string LocalStatus,
        decimal TotalAmount,
        string CurrencyCode,
        string PassengerNames,
        DateTime CreatedAt,
        IReadOnlyList<TripBookingLegDto> Legs);

    public record CancelTripBookingResponseDto(bool Success, string LocalStatus);
}
