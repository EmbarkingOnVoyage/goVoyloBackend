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

    public record SupplierFareTaxDto(string TaxCode, string TaxDesc, decimal TaxAmount);

    public record SupplierFareBreakdownDto(
        decimal BasicAmount,
        decimal AirportTaxAmount,
        IReadOnlyList<SupplierFareTaxDto> Taxes,
        decimal ServiceFeeAmount,
        decimal TradeMarkupAmount,
        decimal PromoDiscount,
        decimal Gst,
        decimal Tds,
        decimal TotalAmount,
        string CurrencyCode);

    public record SupplierBaggageDto(string? CheckInBaggage, string? HandBaggage);

    public record SupplierRescheduleChargeDto(
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
        SupplierFareBreakdownDto FareBreakdown,
        SupplierBaggageDto Baggage,
        IReadOnlyList<SupplierRescheduleChargeDto> RescheduleCharges);

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

    public record SupplierFareRuleDto(string SegmentId, string FareRuleName, string FareRuleDescriptionHtml);

    public record SupplierFareRulesResultDto(IReadOnlyList<SupplierFareRuleDto> Rules);

    public record FlightOfferSession(string SupplierCode, string SearchKey, string FlightKey, string FareId);
}
