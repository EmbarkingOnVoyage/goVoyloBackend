using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITravelerFrequentFlyerRepository
    {
        Task<TravelerFrequentFlyer?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<TravelerFrequentFlyer>> GetByTravelerIdAsync(Guid savedTravelerId);
        Task<bool> ExistsForAirlineAsync(Guid savedTravelerId, string airlineCode);
        Task AddAsync(TravelerFrequentFlyer frequentFlyer);
        Task DeleteAsync(TravelerFrequentFlyer frequentFlyer);
    }
}
