namespace GoVoylo.Domain.Entities
{
    public class UserPreference
    {
        public Guid UserId { get; private set; }
        public string Language { get; private set; } = "en";
        public string Currency { get; private set; } = "INR";
        public DateTime UpdatedAt { get; private set; }

        public UserPreference(Guid userId, string language, string currency)
        {
            UserId = userId;
            Language = language;
            Currency = currency;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private UserPreference()
        {
        }

        public void Update(string language, string currency)
        {
            Language = language;
            Currency = currency;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
