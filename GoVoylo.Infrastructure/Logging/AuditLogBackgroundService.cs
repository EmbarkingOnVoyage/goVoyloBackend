using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Infrastructure.Logging
{
    public class AuditLogBackgroundService : BackgroundService
    {
        private readonly AuditLogQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuditLogBackgroundService> _logger;

        public AuditLogBackgroundService(
            AuditLogQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AuditLogBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var entry in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
                    await repository.AddAsync(new AuditLog(entry.UserId, entry.EventType));
                }
                catch (Exception ex)
                {
                    // An audit-write failure must never take down the request that
                    // triggered it — it already completed. Just log and move on.
                    _logger.LogError(ex, "Failed to persist audit log entry for event {EventType}", entry.EventType);
                }
            }
        }
    }
}
