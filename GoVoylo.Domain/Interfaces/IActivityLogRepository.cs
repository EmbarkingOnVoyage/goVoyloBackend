using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task LogActivityAsync(UserActivityLog log);
}
