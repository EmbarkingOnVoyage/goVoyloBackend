namespace GoVoylo.Application.Interfaces
{
    public interface IAuditService
    {
        // Non-blocking — enqueues the event; the actual DB write happens on a
        // background worker so the caller's request never waits on it.
        void Log(Guid? userId, string eventType, Guid? actorUserId = null);
    }
}
