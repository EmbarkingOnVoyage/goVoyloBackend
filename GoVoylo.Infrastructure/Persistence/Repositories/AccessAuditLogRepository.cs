using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class AccessAuditLogRepository : IAccessAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AccessAuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AccessAuditLog log, CancellationToken cancellationToken)
        {
            await _context.AccessAuditLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(AccessAuditLog log, CancellationToken cancellationToken)
        {
            _context.AccessAuditLogs.Update(log);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<AccessAuditLog?> GetByGatewayOrderIdAsync(string gatewayOrderId, CancellationToken cancellationToken) =>
            _context.AccessAuditLogs
                .Where(x => x.GatewayOrderId == gatewayOrderId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<AccessAuditLog?> GetRecentLookupAsync(string ipAddress, DateTime since, CancellationToken cancellationToken) =>
            _context.AccessAuditLogs
                .AsNoTracking()
                .Where(x => x.IpAddress == ipAddress && x.LookupStatus == AccessAuditLog.LookupDone && x.CreatedAt >= since)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
