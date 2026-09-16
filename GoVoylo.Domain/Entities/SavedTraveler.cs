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
            bool autoAddTravelInsurance = false)
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
            bool autoAddTravelInsurance)
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
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePreferences(string? mealPreference, string? seatPreference)
        {
            MealPreference = mealPreference;
            SeatPreference = seatPreference;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
