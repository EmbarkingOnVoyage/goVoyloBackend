using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.Logging
{
    // Captures the caller's IP and user agent while the request is still alive,
    // then hands the rest to AccessAuditBackgroundService.
    public class AccessAuditService : IAccessAuditService
    {
        private readonly AccessAuditQueue _queue;
        private readonly IClientRequestInfo _client;

        public AccessAuditService(AccessAuditQueue queue, IClientRequestInfo client)
        {
            _queue = queue;
            _client = client;
        }

        public void LogSignIn(Guid userId, string method) =>
            _queue.Enqueue(new SignInAuditEntry(userId, method, _client.IpAddress, Truncate(_client.UserAgent)));

        public void LogPaymentStarted(Guid? userId, string bookingReference, string gatewayOrderId, decimal amount, string currency) =>
            _queue.Enqueue(new PaymentStartedAuditEntry(
                userId, bookingReference, gatewayOrderId, amount, currency, _client.IpAddress, Truncate(_client.UserAgent)));

        public void LogPaymentCompleted(string gatewayOrderId, string? gatewayPaymentId, string status) =>
            _queue.Enqueue(new PaymentCompletedAuditEntry(
                gatewayOrderId, gatewayPaymentId, status, _client.IpAddress, Truncate(_client.UserAgent)));

        private static string? Truncate(string? value) =>
            value != null && value.Length > 500 ? value[..500] : value;
    }
}
