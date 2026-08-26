using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITravelerEmergencyContactRepository
    {
        Task<TravelerEmergencyContact?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<TravelerEmergencyContact>> GetByTravelerIdAsync(Guid savedTravelerId);
        Task AddAsync(TravelerEmergencyContact contact);
        Task UpdateAsync(TravelerEmergencyContact contact);
        Task DeleteAsync(TravelerEmergencyContact contact);
    }
}
