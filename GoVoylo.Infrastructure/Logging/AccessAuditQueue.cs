using System.Threading.Channels;

namespace GoVoylo.Infrastructure.Logging
{
    public abstract record AccessAuditEntry(string? IpAddress, string? UserAgent);

    public record SignInAuditEntry(Guid UserId, string Method, string? IpAddress, string? UserAgent)
        : AccessAuditEntry(IpAddress, UserAgent);

    public record PaymentStartedAuditEntry(
        Guid? UserId, string BookingReference, string GatewayOrderId, decimal Amount, string Currency,
        string? IpAddress, string? UserAgent)
        : AccessAuditEntry(IpAddress, UserAgent);

    public record PaymentCompletedAuditEntry(
        string GatewayOrderId, string? GatewayPaymentId, string Status, string? IpAddress, string? UserAgent)
        : AccessAuditEntry(IpAddress, UserAgent);

    // Singleton in-process queue — AccessAuditService (scoped) writes to it,
    // AccessAuditBackgroundService (one long-running worker) drains it in order,
    // so a payment's "started" row is always saved before its "completed" update.
    public class AccessAuditQueue
    {
        private readonly Channel<AccessAuditEntry> _channel = Channel.CreateUnbounded<AccessAuditEntry>();

        public ChannelReader<AccessAuditEntry> Reader => _channel.Reader;

        public void Enqueue(AccessAuditEntry entry) => _channel.Writer.TryWrite(entry);
    }
}
