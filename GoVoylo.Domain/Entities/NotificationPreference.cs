namespace GoVoylo.Domain.Entities
{
    public class NotificationPreference
    {
        public Guid UserId { get; private set; }

        // Transactional email has no setter — it can never be disabled (GV-CUST-BE-011 rule).
        public bool EmailTransactional { get; private set; } = true;
        public bool EmailMarketing { get; private set; } = true;
        public bool SmsTransactional { get; private set; } = true;
        public bool SmsMarketing { get; private set; }
        public bool PushEnabled { get; private set; } = true;
        public DateTime UpdatedAt { get; private set; }

        public NotificationPreference(Guid userId)
        {
            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private NotificationPreference()
        {
        }

        public void Update(bool emailMarketing, bool smsTransactional, bool smsMarketing, bool pushEnabled)
        {
            EmailMarketing = emailMarketing;
            SmsTransactional = smsTransactional;
            SmsMarketing = smsMarketing;
            PushEnabled = pushEnabled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
