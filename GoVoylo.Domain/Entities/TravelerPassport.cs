using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerPassport : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public byte[] PassportNumberEncrypted { get; private set; } = null!;
        public string IssuingCountry { get; private set; } = null!;
        public DateTime ExpiryDate { get; private set; }
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
            PassportNumberEncrypted = passportNumberEncrypted;
            IssuingCountry = issuingCountry;
            ExpiryDate = expiryDate;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
