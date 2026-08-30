using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IAirportRepository
    {
        Task<IReadOnlyList<Airport>> SearchAsync(string query, int limit);
        Task<Airport?> GetByIataAsync(string iataCode);
        Task<IReadOnlyList<Airport>> GetPopularAsync();
        Task<int> CountAsync();
        Task AddAsync(Airport airport);
        Task UpdateAsync(Airport airport);
    }
}
