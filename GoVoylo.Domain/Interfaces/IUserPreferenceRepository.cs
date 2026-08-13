using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IUserPreferenceRepository
    {
        Task<UserPreference?> GetByUserIdAsync(Guid userId);
        Task UpsertAsync(UserPreference preference);
    }
}
