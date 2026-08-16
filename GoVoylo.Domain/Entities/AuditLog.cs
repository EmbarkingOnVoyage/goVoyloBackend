using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        // The account this event happened to. Nullable — some security events
        // (e.g. a failed login against an email that doesn't exist) have no
        // resolvable user to attribute the event to.
        public Guid? UserId { get; private set; }

        // Who performed the action, when different from UserId — e.g. an admin
        // changing a customer's status. Null means self-initiated (login, logout,
        // password change, etc. — the actor and the subject are the same person).
        public Guid? ActorUserId { get; private set; }

        // registration | login_success | login_failed | logout | password_changed
        // | customer_status_changed
        public string EventType { get; private set; } = null!;

        public AuditLog(Guid? userId, string eventType, Guid? actorUserId = null)
        {
            UserId = userId;
            EventType = eventType;
            ActorUserId = actorUserId;
        }

        // Required by EF Core
        private AuditLog()
        {
        }
    }
}
