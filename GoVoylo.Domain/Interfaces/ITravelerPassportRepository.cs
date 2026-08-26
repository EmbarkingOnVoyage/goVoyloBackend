using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITravelerPassportRepository
    {
        Task<TravelerPassport?> GetByTravelerIdAsync(Guid savedTravelerId);
        Task<IReadOnlyList<TravelerPassport>> GetExpiringUnnotifiedAsync(DateTime windowEnd);
        Task AddAsync(TravelerPassport passport);
        Task UpdateAsync(TravelerPassport passport);
        Task DeleteAsync(TravelerPassport passport);
    }
}
