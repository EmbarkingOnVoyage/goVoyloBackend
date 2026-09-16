using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.Logging
{
    public class AuditService : IAuditService
    {
        private readonly AuditLogQueue _queue;

        public AuditService(AuditLogQueue queue)
        {
            _queue = queue;
        }

        public void Log(Guid? userId, string eventType, Guid? actorUserId = null)
        {
            _queue.Enqueue(userId, eventType, actorUserId);
        }
    }
}
