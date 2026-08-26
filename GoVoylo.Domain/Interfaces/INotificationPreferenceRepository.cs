using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface INotificationPreferenceRepository
    {
        Task<NotificationPreference?> GetByUserIdAsync(Guid userId);
        Task UpsertAsync(NotificationPreference preference);
    }
}
