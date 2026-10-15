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
        decimal? MaxFeePerPax,
        // Journeys charged at FirstJourneyPercent before ExtraJourneyPercent applies,
        // and whether the group factor (PaxBands) applies — see ConvenienceFeeTripRate.
        int FullRateJourneys = 1,
        bool ApplyPaxBands = true);

    public record ConvenienceFeePaxBandDto(int MinPax, decimal Factor);

    // FirstBookingFree: this customer's next booking has no convenience fee.
    public record ConvenienceFeeOfferDto(bool FirstBookingFree, DateTimeOffset? ValidUntil);
}
