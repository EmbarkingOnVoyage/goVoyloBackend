using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        // Nullable — some security events (e.g. a failed login against an email
        // that doesn't exist) have no resolvable user to attribute the event to.
        public Guid? UserId { get; private set; }

        // registration | login_success | login_failed | logout | password_changed
        public string EventType { get; private set; } = null!;

        public AuditLog(Guid? userId, string eventType)
        {
            UserId = userId;
            EventType = eventType;
        }

        // Required by EF Core
        private AuditLog()
        {
        }
    }
}
