using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerVisa : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public string Country { get; private set; } = null!;
        public byte[] VisaNumberEncrypted { get; private set; } = null!;
        public string? VisaType { get; private set; }
        public DateTime? IssueDate { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public TravelerVisa(
            Guid savedTravelerId,
            string country,
            byte[] visaNumberEncrypted,
            string? visaType,
            DateTime? issueDate,
            DateTime expiryDate)
        {
            SavedTravelerId = savedTravelerId;
            Country = country;
            VisaNumberEncrypted = visaNumberEncrypted;
            VisaType = visaType;
            IssueDate = issueDate;
            ExpiryDate = expiryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private TravelerVisa()
        {
        }

        public void Update(byte[] visaNumberEncrypted, string? visaType, DateTime? issueDate, DateTime expiryDate)
        {
            VisaNumberEncrypted = visaNumberEncrypted;
            VisaType = visaType;
            IssueDate = issueDate;
            ExpiryDate = expiryDate;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
