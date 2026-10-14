using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // How much of the supplier's commission is passed to the customer as a
    // discount. Prices show the fare plus our commission struck out, and charge it
    // minus Percent of that commission. The rate in force is the newest row whose
    // EffectiveFrom has passed; to change it, add a row (old rows stay as history).
    public class CommissionDiscountRate : BaseEntity
    {
        public decimal Percent { get; private set; }
        public DateTime EffectiveFrom { get; private set; }
        public string? Note { get; private set; }

        public CommissionDiscountRate(decimal percent, DateTime effectiveFrom, string? note = null)
        {
            if (percent < 0 || percent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be between 0 and 100.");
            }
            Percent = percent;
            EffectiveFrom = effectiveFrom;
            Note = note;
        }

        // Required by EF Core
        private CommissionDiscountRate()
        {
        }
    }
}
