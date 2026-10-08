using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // Convenience fee rate for one trip type, as a percentage of each chargeable
    // passenger's base fare per journey: the first journey at FirstJourneyPercent,
    // every further journey at ExtraJourneyPercent (OneWay 1.00; RoundTrip 0.80 +
    // 0.80; MultiCity 1.00 + 0.75 each extra). Edited in the database, no deploy.
    public class ConvenienceFeeTripRate : BaseEntity
    {
        public string TripType { get; private set; } = null!;
        public decimal FirstJourneyPercent { get; private set; }
        public decimal ExtraJourneyPercent { get; private set; }

        // Most one passenger can be charged for the whole trip; null = no cap.
        public decimal? MaxFeePerPax { get; private set; }

        public bool IsActive { get; private set; } = true;
        public DateTime UpdatedAt { get; private set; }

        public ConvenienceFeeTripRate(
            string tripType, decimal firstJourneyPercent, decimal extraJourneyPercent, decimal? maxFeePerPax = null)
        {
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
            journeyIndex == 0 ? FirstJourneyPercent : ExtraJourneyPercent;
    }
}
