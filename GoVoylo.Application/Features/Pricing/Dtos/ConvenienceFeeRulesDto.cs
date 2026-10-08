namespace GoVoylo.Application.Features.Pricing.Dtos
{
    // The rules the app needs to show the convenience fee before booking — the
    // server recalculates it at booking time (see CreateBookingCommandHandler).
    public record ConvenienceFeeRulesDto(
        IReadOnlyList<ConvenienceFeeTripRateDto> TripRates,
        IReadOnlyList<ConvenienceFeePaxBandDto> PaxBands);

    public record ConvenienceFeeTripRateDto(
        string TripType,
        decimal FirstJourneyPercent,
        decimal ExtraJourneyPercent,
        decimal? MaxFeePerPax);

    public record ConvenienceFeePaxBandDto(int MinPax, decimal Factor);
}
