namespace GoVoylo.Application.Interfaces
{
    // Airport reference data changes rarely (admin toggles, periodic import), so a short
    // TTL is enough to absorb autocomplete-heavy traffic without needing invalidation logic.
    public interface IAirportCacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory);
    }
}
