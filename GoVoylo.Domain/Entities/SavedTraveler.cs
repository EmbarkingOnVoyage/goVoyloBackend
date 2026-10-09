using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class SavedTraveler : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string TravelerType { get; private set; } = null!; // adult | child | infant
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public DateTime DateOfBirth { get; private set; }
        public string? Gender { get; private set; }
        public string? Nationality { get; private set; }
        public string? MealPreference { get; private set; }
        public string? SeatPreference { get; private set; } // window | aisle | middle
        public string? City { get; private set; }
        public string? State { get; private set; }

        // Optional contact details for this traveller (the booking's own contact is
        // the account holder's — see CreateBookingCommand.PassengerEmail/Mobile).
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? PhoneCountryCode { get; private set; }

        // The signed-in customer themselves (one per account). Their email is always
        // the account's sign-in email — the primary email — and they can't be removed.
        public bool IsAccountHolder { get; private set; }
        public bool AutoAddTravelInsurance { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public SavedTraveler(
            Guid userId,
            string travelerType,
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            string? gender,
            string? nationality,
            string? city = null,
            string? state = null,
            bool autoAddTravelInsurance = false,
            string? email = null,
            string? phone = null,
            string? phoneCountryCode = null)
        {
            UserId = userId;
            TravelerType = travelerType;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Nationality = nationality;
            City = city;
            State = state;
            AutoAddTravelInsurance = autoAddTravelInsurance;
            SetContact(email, phone, phoneCountryCode);
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private SavedTraveler()
        {
        }

        public void Update(
            string travelerType,
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            string? gender,
            string? nationality,
            string? city,
            string? state,
            bool autoAddTravelInsurance,
            string? email = null,
            string? phone = null,
            string? phoneCountryCode = null)
        {
            TravelerType = travelerType;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Nationality = nationality;
            City = city;
            State = state;
            AutoAddTravelInsurance = autoAddTravelInsurance;
            SetContact(email, phone, phoneCountryCode);
            UpdatedAt = DateTime.UtcNow;
        }

        // Blank values clear the field; a phone without a country code is Indian.
        private void SetContact(string? email, string? phone, string? phoneCountryCode)
        {
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            PhoneCountryCode = Phone == null
                ? null
                : string.IsNullOrWhiteSpace(phoneCountryCode) ? "+91" : phoneCountryCode.Trim();
        }

        public void UpdatePreferences(string? mealPreference, string? seatPreference)
        {
            MealPreference = mealPreference;
            SeatPreference = seatPreference;
            UpdatedAt = DateTime.UtcNow;
        }

        // Makes this the account holder's own traveller, with the account email as
        // its (primary) email.
        public void MarkAsAccountHolder(string? accountEmail)
        {
            IsAccountHolder = true;
            if (!string.IsNullOrWhiteSpace(accountEmail))
            {
                Email = accountEmail.Trim();
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
