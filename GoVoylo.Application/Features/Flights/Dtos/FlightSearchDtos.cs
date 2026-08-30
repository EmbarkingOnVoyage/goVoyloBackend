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

    public record FareTaxDto(string TaxCode, string TaxDesc, decimal TaxAmount);

    public record FareBreakdownDto(
        decimal BasicAmount,
        decimal AirportTaxAmount,
        IReadOnlyList<FareTaxDto> Taxes,
        decimal ServiceFeeAmount,
        decimal TradeMarkupAmount,
        decimal PromoDiscount,
        decimal Gst,
        decimal Tds,
        decimal TotalAmount,
        string CurrencyCode);

    public record BaggageDto(string? CheckInBaggage, string? HandBaggage);

    public record RescheduleChargeDto(
        int PassengerType,
        string? Value,
        int ValueType,
        int DurationFrom,
        int DurationTo,
        int DurationTypeFrom,
        int DurationTypeTo,
        decimal OnlineServiceFee,
        decimal OfflineServiceFee,
        string? Remarks);

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
        FareBreakdownDto FareBreakdown,
        BaggageDto Baggage,
        IReadOnlyList<RescheduleChargeDto> RescheduleCharges);

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

    public record SearchHistoryDto(
        Guid SearchLogId,
        string Origin,
        string Destination,
        DateTime TravelDate,
        string TripType,
        string CabinClass,
        DateTime SearchedAt);

    public record RouteDto(string Origin, string Destination, int SearchCount);

    public record FareRuleDto(string SegmentId, string FareRuleName, string FareRuleDescriptionHtml);

    public record FareRulesResponseDto(Guid OfferId, IReadOnlyList<FareRuleDto> Rules);
}
