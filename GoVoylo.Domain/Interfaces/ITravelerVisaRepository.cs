using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITravelerVisaRepository
    {
        Task<TravelerVisa?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<TravelerVisa>> GetByTravelerIdAsync(Guid savedTravelerId);
        Task<bool> ExistsForCountryAsync(Guid savedTravelerId, string country);
        Task AddAsync(TravelerVisa visa);
        Task UpdateAsync(TravelerVisa visa);
        Task DeleteAsync(TravelerVisa visa);
    }
}
