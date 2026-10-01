using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerPassport : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public byte[] PassportNumberEncrypted { get; private set; } = null!;
        public string IssuingCountry { get; private set; } = null!;
        public DateTime ExpiryDate { get; private set; }

        // Optional — some fares require it (Tripjack Review conditions.pcs.pid);
        // passports saved before it was collected won't have one.
        public DateTime? IssueDate { get; private set; }
        public DateTime? LastExpiryAlertSentAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public TravelerPassport(
            Guid savedTravelerId,
            byte[] passportNumberEncrypted,
            string issuingCountry,
            DateTime expiryDate,
            DateTime? issueDate = null)
        {
            SavedTravelerId = savedTravelerId;
            PassportNumberEncrypted = passportNumberEncrypted;
            IssuingCountry = issuingCountry;
            ExpiryDate = expiryDate;
            IssueDate = issueDate;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private TravelerPassport()
        {
        }

        public void Update(
            byte[] passportNumberEncrypted, string issuingCountry, DateTime expiryDate, DateTime? issueDate = null)
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
            IssueDate = issueDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkExpiryAlertSent()
        {
            LastExpiryAlertSentAt = DateTime.UtcNow;
        }
    }
}
