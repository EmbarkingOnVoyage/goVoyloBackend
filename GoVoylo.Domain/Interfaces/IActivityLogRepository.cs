using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task LogActivityAsync(UserActivityLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserActivityLog>> GetByUserIdAsync(string userId, int limit, CancellationToken cancellationToken = default);
}
