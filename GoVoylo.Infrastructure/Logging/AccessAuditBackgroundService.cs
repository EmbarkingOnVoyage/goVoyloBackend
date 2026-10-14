using System.Net;
using System.Net.Sockets;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Infrastructure.Logging
{
    // Saves access-audit rows, looks up each IP's location and safety, and asks
    // Razorpay how a completed payment was made. Lookups are reused for the same
    // IP (a day in memory, a week from the table) to stay inside the
    // IPQualityScore quota.
    public class AccessAuditBackgroundService : BackgroundService
    {
        private static readonly TimeSpan MemoryReuse = TimeSpan.FromHours(24);
        private static readonly TimeSpan DatabaseReuse = TimeSpan.FromDays(7);

        private readonly AccessAuditQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AccessAuditBackgroundService> _logger;

        public AccessAuditBackgroundService(
            AccessAuditQueue queue,
            IServiceScopeFactory scopeFactory,
            IMemoryCache cache,
            ILogger<AccessAuditBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _cache = cache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var entry in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await ProcessAsync(entry, scope.ServiceProvider, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // The request that raised this already completed — log and move on.
                    _logger.LogError(ex, "Failed to save access audit entry {EntryType}", entry.GetType().Name);
                }
            }
        }

        public async Task ProcessAsync(AccessAuditEntry entry, IServiceProvider services, CancellationToken ct)
        {
            var repository = services.GetRequiredService<IAccessAuditLogRepository>();

            switch (entry)
            {
                case SignInAuditEntry signIn:
                {
                    var log = AccessAuditLog.SignIn(signIn.UserId, signIn.Method, signIn.IpAddress, signIn.UserAgent);
                    await LookUpAsync(log, services, repository, ct);
                    await repository.AddAsync(log, ct);
                    break;
                }
                case PaymentStartedAuditEntry started:
                {
                    var log = AccessAuditLog.PaymentStarted(
                        started.UserId, started.BookingReference, started.GatewayOrderId, started.Amount, started.Currency,
                        started.IpAddress, started.UserAgent);
                    await LookUpAsync(log, services, repository, ct);
                    await repository.AddAsync(log, ct);
                    break;
                }
                case PaymentCompletedAuditEntry completed:
                {
                    string? method = null, detail = null;
                    if (!string.IsNullOrWhiteSpace(completed.GatewayPaymentId))
                    {
                        var paid = await services.GetRequiredService<IRazorpayClient>()
                            .GetPaymentMethodAsync(completed.GatewayPaymentId, ct);
                        method = paid?.Method;
                        detail = paid?.Detail;
                    }

                    var log = await repository.GetByGatewayOrderIdAsync(completed.GatewayOrderId, ct);
                    if (log != null)
                    {
                        log.CompletePayment(completed.Status, completed.GatewayPaymentId, method, detail);
                        await repository.UpdateAsync(log, ct);
                        break;
                    }

                    // An order created before this audit existed: record what we
                    // can from the verify request.
                    var payment = await services.GetRequiredService<IPaymentRepository>()
                        .GetByGatewayOrderIdAsync(completed.GatewayOrderId, ct);
                    log = AccessAuditLog.PaymentStarted(
                        null, payment?.BookingReference ?? "", completed.GatewayOrderId, payment?.TotalAmount ?? 0m,
                        payment?.Currency ?? "INR", completed.IpAddress, completed.UserAgent);
                    log.CompletePayment(completed.Status, completed.GatewayPaymentId, method, detail);
                    await LookUpAsync(log, services, repository, ct);
                    await repository.AddAsync(log, ct);
                    break;
                }
            }
        }

        private async Task LookUpAsync(
            AccessAuditLog log, IServiceProvider services, IAccessAuditLogRepository repository, CancellationToken ct)
        {
            var ip = log.IpAddress;
            if (ip == null || !IsPublic(ip))
            {
                log.SetLookup(AccessAuditLog.LookupPrivateIp);
                return;
            }

            var client = services.GetRequiredService<IIpReputationClient>();
            if (!client.IsConfigured)
            {
                log.SetLookup(AccessAuditLog.LookupNotConfigured);
                return;
            }

            var cacheKey = $"access-audit-ip:{ip}";
            if (_cache.TryGetValue(cacheKey, out IpReputation? cached) && cached != null)
            {
                Apply(log, cached);
                return;
            }

            var previous = await repository.GetRecentLookupAsync(ip, DateTime.UtcNow - DatabaseReuse, ct);
            if (previous != null)
            {
                log.CopyLookupFrom(previous);
                return;
            }

            var reputation = await client.LookupAsync(ip, ct);
            if (reputation == null)
            {
                log.SetLookup(AccessAuditLog.LookupFailed);
                return;
            }

            _cache.Set(cacheKey, reputation, MemoryReuse);
            Apply(log, reputation);
        }

        private static void Apply(AccessAuditLog log, IpReputation r) => log.SetLookup(
            AccessAuditLog.LookupDone, r.City, r.Region, r.CountryCode, r.Isp, r.Latitude, r.Longitude,
            r.IsVpn, r.IsProxy, r.IsTor, r.RecentAbuse, r.IsBot, r.FraudScore, r.IsSecure);

        // Loopback, private (10/8, 172.16/12, 192.168/16, fc00::/7), link-local
        // and CGNAT (100.64/10) addresses can't be looked up.
        public static bool IsPublic(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip)) return false;
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
            if (IPAddress.IsLoopback(ip)) return false;

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var b = ip.GetAddressBytes();
                return !(b[0] == 10
                    || b[0] == 0
                    || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                    || (b[0] == 192 && b[1] == 168)
                    || (b[0] == 169 && b[1] == 254)
                    || (b[0] == 100 && b[1] >= 64 && b[1] <= 127));
            }

            return !(ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal || ip.Equals(IPAddress.IPv6None));
        }
    }
}
