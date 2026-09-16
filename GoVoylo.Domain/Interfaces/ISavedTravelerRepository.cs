using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ISavedTravelerRepository
    {
        Task<SavedTraveler?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<SavedTraveler>> GetByUserIdAsync(Guid userId);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<bool> ExistsByIdentityAsync(Guid userId, string firstName, string lastName, DateTime dateOfBirth);
        Task AddAsync(SavedTraveler traveler);
        Task UpdateAsync(SavedTraveler traveler);
    }
}
