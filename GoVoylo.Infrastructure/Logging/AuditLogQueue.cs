using System.Threading.Channels;

namespace GoVoylo.Infrastructure.Logging
{
    public record AuditLogEntry(Guid? UserId, string EventType, Guid? ActorUserId = null);

    // Singleton in-process queue — AuditService (scoped, one per request) writes to
    // it, AuditLogBackgroundService (a single long-running worker) drains it.
    public class AuditLogQueue
    {
        private readonly Channel<AuditLogEntry> _channel = Channel.CreateUnbounded<AuditLogEntry>();

        public ChannelReader<AuditLogEntry> Reader => _channel.Reader;

        public void Enqueue(Guid? userId, string eventType, Guid? actorUserId = null)
        {
            _channel.Writer.TryWrite(new AuditLogEntry(userId, eventType, actorUserId));
        }
    }
}
