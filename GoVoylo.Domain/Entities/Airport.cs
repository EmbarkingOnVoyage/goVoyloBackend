using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class Airport : BaseEntity
    {
        public string IataCode { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public string Country { get; private set; } = null!;
        public bool IsPopular { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime UpdatedAt { get; private set; }

        public Airport(string iataCode, string name, string city, string country, bool isPopular = false)
        {
            IataCode = iataCode.ToUpperInvariant();
            Name = name;
            City = city;
            Country = country;
            IsPopular = isPopular;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private Airport()
        {
        }

        public void Upsert(string name, string city, string country)
        {
            Name = name;
            City = city;
            Country = country;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPopular(bool isPopular)
        {
            IsPopular = isPopular;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
