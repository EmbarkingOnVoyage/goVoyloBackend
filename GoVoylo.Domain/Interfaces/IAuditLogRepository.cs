using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
        Task<(IReadOnlyList<AuditLog> Logs, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize);
    }
}
