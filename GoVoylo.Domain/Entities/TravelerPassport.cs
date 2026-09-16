using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerPassport : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public byte[] PassportNumberEncrypted { get; private set; } = null!;
        public string IssuingCountry { get; private set; } = null!;
        public DateTime ExpiryDate { get; private set; }
        public DateTime? LastExpiryAlertSentAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public TravelerPassport(
            Guid savedTravelerId,
            byte[] passportNumberEncrypted,
            string issuingCountry,
            DateTime expiryDate)
        {
            SavedTravelerId = savedTravelerId;
            PassportNumberEncrypted = passportNumberEncrypted;
            IssuingCountry = issuingCountry;
            ExpiryDate = expiryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private TravelerPassport()
        {
        }

        public void Update(byte[] passportNumberEncrypted, string issuingCountry, DateTime expiryDate)
        {
            if (expiryDate != ExpiryDate)
            {
                // A changed expiry date (e.g. renewal) means any prior alert no
                // longer applies to the current date — allow re-alerting on it.
                LastExpiryAlertSentAt = null;
            }

            PassportNumberEncrypted = passportNumberEncrypted;
            IssuingCountry = issuingCountry;
            ExpiryDate = expiryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkExpiryAlertSent()
        {
            LastExpiryAlertSentAt = DateTime.UtcNow;
        }
    }
}
