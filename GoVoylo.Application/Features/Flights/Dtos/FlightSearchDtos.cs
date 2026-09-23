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
}
