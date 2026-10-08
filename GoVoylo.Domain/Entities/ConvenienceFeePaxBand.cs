using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // Passenger taper on the convenience fee: from MinPax chargeable passengers
    // (adults + children; infants are never charged) the fee is multiplied by
    // Factor — 1 pax 1.00, 3 pax 0.85, 6 pax 0.75. The band with the highest
    // MinPax not above the passenger count applies.
    public class ConvenienceFeePaxBand : BaseEntity
    {
        public int MinPax { get; private set; }
        public decimal Factor { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime UpdatedAt { get; private set; }

        public ConvenienceFeePaxBand(int minPax, decimal factor)
        {
            MinPax = minPax;
            Factor = factor;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private ConvenienceFeePaxBand()
        {
        }
    }
}
