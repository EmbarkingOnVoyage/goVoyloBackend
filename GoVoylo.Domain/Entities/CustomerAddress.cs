using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class CustomerAddress : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string? Label { get; private set; }
        public string Line1 { get; private set; } = null!;
        public string? Line2 { get; private set; }
        public string City { get; private set; } = null!;
        public string State { get; private set; } = null!;
        public string PostalCode { get; private set; } = null!;
        public string Country { get; private set; } = "IN";
        public bool IsDefault { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public CustomerAddress(
            Guid userId,
            string? label,
            string line1,
            string? line2,
            string city,
            string state,
            string postalCode,
            string country,
            bool isDefault)
        {
            UserId = userId;
            Label = label;
            Line1 = line1;
            Line2 = line2;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
            IsDefault = isDefault;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private CustomerAddress()
        {
        }

        public void Update(
            string? label,
            string line1,
            string? line2,
            string city,
            string state,
            string postalCode,
            string country)
        {
            Label = label;
            Line1 = line1;
            Line2 = line2;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsDefault(bool isDefault)
        {
            IsDefault = isDefault;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
