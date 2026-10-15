using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Pricing
{
    // What the adults and children pay for one journey together: their fares before
    // the commission discount (base + taxes) plus their add-ons on it. Infants are
    // left out — they're never charged the fee. A round trip has two journeys, a
    // multi-city trip one per city pair.
    public record JourneyFare(decimal ChargeableAmount);

    public static class ConvenienceFeeCalculator
    {
        // Each journey's chargeable amount times its rate (see
        // ConvenienceFeeTripRate.PercentForJourney), times the group factor for
        // the number of adults + children when the rate applies bands, capped per
        // passenger when the rate has a cap. Rounded to 2 decimals.
        public static decimal Calculate(
            ConvenienceFeeTripRate tripRate,
            IReadOnlyCollection<ConvenienceFeePaxBand> paxBands,
            IReadOnlyList<JourneyFare> journeys,
            int adultCount,
            int childCount)
        {
            var chargeablePax = adultCount + childCount;
            if (chargeablePax <= 0 || journeys.Count == 0)
            {
                return 0m;
            }

            var factor = !tripRate.ApplyPaxBands
                ? 1m
                : paxBands
                    .Where(b => b.MinPax <= chargeablePax)
                    .OrderByDescending(b => b.MinPax)
                    .Select(b => b.Factor)
                    .FirstOrDefault(1m);

            var total = journeys
                .Select((journey, index) => journey.ChargeableAmount * tripRate.PercentForJourney(index) / 100m)
                .Sum() * factor;

            if (tripRate.MaxFeePerPax is { } cap)
            {
                total = Math.Min(total, cap * chargeablePax);
            }

            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }
    }
}
