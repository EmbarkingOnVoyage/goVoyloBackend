namespace GoVoylo.Application.Features.Flights.Dtos
{
    public record SupplierFlightSegmentDto(
        string Origin,
        string Destination,
        string AirlineCode,
        string FlightNumber,
        DateTime DepartureDateTime,
        DateTime ArrivalDateTime,
        string Duration);

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
        int SeatsAvailable);

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
