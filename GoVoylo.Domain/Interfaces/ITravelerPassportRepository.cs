using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITravelerPassportRepository
    {
        Task<TravelerPassport?> GetByTravelerIdAsync(Guid savedTravelerId);
        Task AddAsync(TravelerPassport passport);
        Task UpdateAsync(TravelerPassport passport);
        Task DeleteAsync(TravelerPassport passport);
    }
}
