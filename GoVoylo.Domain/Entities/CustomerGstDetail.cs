using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class CustomerGstDetail : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Gstin { get; private set; } = null!;
        public string LegalName { get; private set; } = null!;
        public string? TradeName { get; private set; }
        public bool IsVerified { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public CustomerGstDetail(Guid userId, string gstin, string legalName, string? tradeName)
        {
            UserId = userId;
            Gstin = gstin;
            LegalName = legalName;
            TradeName = tradeName;
            IsVerified = false;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private CustomerGstDetail()
        {
        }

        public void Update(string gstin, string legalName, string? tradeName)
        {
            Gstin = gstin;
            LegalName = legalName;
            TradeName = tradeName;
            IsVerified = false; // re-verify whenever GST details change
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
