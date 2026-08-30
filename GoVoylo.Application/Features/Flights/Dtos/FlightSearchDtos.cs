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
        string FlightNumber,
        DateTime DepartureDateTime,
        DateTime ArrivalDateTime,
        string Duration);

    public record FlightOfferDto(
        Guid OfferId,
        string AirlineCode,
        string AirlineName,
        bool Refundable,
        bool IsLowCostCarrier,
        IReadOnlyList<FlightOfferSegmentDto> Segments,
        decimal TotalAmount,
        string CurrencyCode,
        int SeatsAvailable);

    // SearchId anchors the result set so filter/sort/summary calls can operate on it
    // without re-calling the supplier — see IFlightSearchResultCache.
    public record FlightSearchResponseDto(Guid SearchId, IReadOnlyList<FlightOfferDto> Offers);

    public record FlightRepriceResponseDto(
        Guid OfferId,
        decimal TotalAmount,
        string CurrencyCode,
        bool PriceChanged);

    public record FlightOfferFilterRequestDto(
        Guid SearchId,
        decimal? MinPrice,
        decimal? MaxPrice,
        IReadOnlyList<string>? AirlineCodes,
        IReadOnlyList<int>? StopCounts,
        TimeOnly? DepartureTimeFrom,
        TimeOnly? DepartureTimeTo,
        TimeOnly? ArrivalTimeFrom,
        TimeOnly? ArrivalTimeTo,
        int? MaxDurationMinutes,
        bool? RefundableOnly,
        string? SortBy);

    public record FilterSummaryDto(
        decimal MinPrice,
        decimal MaxPrice,
        IReadOnlyList<string> AirlineCodes,
        IReadOnlyList<int> StopCounts,
        int MinDurationMinutes,
        int MaxDurationMinutes,
        int OfferCount);
}
