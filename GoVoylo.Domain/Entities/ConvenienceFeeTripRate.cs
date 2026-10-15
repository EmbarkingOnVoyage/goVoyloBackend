using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // Convenience fee rate for one trip type, as a percentage of what the adults and
    // children pay for each journey (fare before the commission discount, plus
    // their add-ons): the first FullRateJourneys journeys at FirstJourneyPercent,
    // every further one at ExtraJourneyPercent. ApplyPaxBands applies the group
    // factor (ConvenienceFeePaxBand). Seeded: OneWay / RoundTrip 2.64% with bands;
    // MultiCity 2.64% for the first two journeys, 1.98% (75%) after, no bands.
    // Edited in the database, no deploy.
    public class ConvenienceFeeTripRate : BaseEntity
    {
        public string TripType { get; private set; } = null!;
        public decimal FirstJourneyPercent { get; private set; }
        public decimal ExtraJourneyPercent { get; private set; }

        // Most one passenger can be charged for the whole trip; null = no cap.
        public decimal? MaxFeePerPax { get; private set; }

        public int FullRateJourneys { get; private set; } = 1;
        public bool ApplyPaxBands { get; private set; } = true;

        public bool IsActive { get; private set; } = true;
        public DateTime UpdatedAt { get; private set; }

        public ConvenienceFeeTripRate(
            string tripType, decimal firstJourneyPercent, decimal extraJourneyPercent, decimal? maxFeePerPax = null,
            int fullRateJourneys = 1, bool applyPaxBands = true)
        {
            FullRateJourneys = Math.Max(1, fullRateJourneys);
            ApplyPaxBands = applyPaxBands;
            TripType = tripType;
            FirstJourneyPercent = firstJourneyPercent;
            ExtraJourneyPercent = extraJourneyPercent;
            MaxFeePerPax = maxFeePerPax;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private ConvenienceFeeTripRate()
        {
        }

        public decimal PercentForJourney(int journeyIndex) =>
            journeyIndex < FullRateJourneys ? FirstJourneyPercent : ExtraJourneyPercent;
    }
}
