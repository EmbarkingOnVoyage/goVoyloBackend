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

    public record FlightOfferSession(string SupplierCode, string SearchKey, string FlightKey, string FareId);

    public record SupplierLowFareRequestDto(string Origin, string Destination, int Month, int Year);

    public record SupplierLowFareDayDto(
        DateTime TravelDate,
        decimal Amount,
        string CurrencyCode,
        string AirlineCode,
        string AirlineName);

    public record SupplierLowFareResultDto(IReadOnlyList<SupplierLowFareDayDto> Days);
}
