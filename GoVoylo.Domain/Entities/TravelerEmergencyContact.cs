using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerEmergencyContact : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Relationship { get; private set; }
        public string Phone { get; private set; } = null!;
        public string PhoneCountryCode { get; private set; } = null!;
        public string? Email { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public TravelerEmergencyContact(
            Guid savedTravelerId,
            string name,
            string? relationship,
            string phone,
            string phoneCountryCode,
            string? email)
        {
            SavedTravelerId = savedTravelerId;
            Name = name;
            Relationship = relationship;
            Phone = phone;
            PhoneCountryCode = phoneCountryCode;
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private TravelerEmergencyContact()
        {
        }

        public void Update(string name, string? relationship, string phone, string phoneCountryCode, string? email)
        {
            Name = name;
            Relationship = relationship;
            Phone = phone;
            PhoneCountryCode = phoneCountryCode;
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
