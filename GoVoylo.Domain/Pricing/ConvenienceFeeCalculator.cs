using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Pricing
{
    // One journey's base fare (before taxes and fees) for a single adult and a
    // single child — a round trip has two journeys, a multi-city trip one per city pair.
    public record JourneyBaseFare(decimal AdultBaseFare, decimal ChildBaseFare);

    public static class ConvenienceFeeCalculator
    {
        // Fee for the whole booking: each chargeable passenger pays their base fare
        // of every journey times that journey's rate (first journey vs. extra
        // journeys), times the passenger band's factor, capped per passenger when the
        // trip rate has a cap. Infants are never charged and don't count towards the
        // band. Rounded to 2 decimals.
        public static decimal Calculate(
            ConvenienceFeeTripRate tripRate,
            IReadOnlyCollection<ConvenienceFeePaxBand> paxBands,
            IReadOnlyList<JourneyBaseFare> journeys,
            int adultCount,
            int childCount)
        {
            var chargeablePax = adultCount + childCount;
            if (chargeablePax <= 0 || journeys.Count == 0)
            {
                return 0m;
            }

            var factor = paxBands
                .Where(b => b.MinPax <= chargeablePax)
                .OrderByDescending(b => b.MinPax)
                .Select(b => b.Factor)
                .FirstOrDefault(1m);

            decimal PerPax(Func<JourneyBaseFare, decimal> baseFare)
            {
                var fee = journeys
                    .Select((journey, index) => baseFare(journey) * tripRate.PercentForJourney(index) / 100m)
                    .Sum() * factor;
                return tripRate.MaxFeePerPax is { } cap ? Math.Min(fee, cap) : fee;
            }

            var total = PerPax(j => j.AdultBaseFare) * adultCount + PerPax(j => j.ChildBaseFare) * childCount;
            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }
    }
}
