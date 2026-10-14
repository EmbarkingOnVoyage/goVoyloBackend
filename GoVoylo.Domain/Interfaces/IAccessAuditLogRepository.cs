using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IAccessAuditLogRepository
    {
        Task AddAsync(AccessAuditLog log, CancellationToken cancellationToken);
        Task UpdateAsync(AccessAuditLog log, CancellationToken cancellationToken);
        Task<AccessAuditLog?> GetByGatewayOrderIdAsync(string gatewayOrderId, CancellationToken cancellationToken);

        // The newest row for this IP whose lookup finished since `since` — reused
        // so the same IP isn't looked up (and billed) again.
        Task<AccessAuditLog?> GetRecentLookupAsync(string ipAddress, DateTime since, CancellationToken cancellationToken);
    }
}
