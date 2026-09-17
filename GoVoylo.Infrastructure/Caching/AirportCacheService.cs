using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.Caching
{
    public class AirportCacheService : IAirportCacheService
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

        private readonly IMemoryCache _cache;

        public AirportCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
        {
            if (_cache.TryGetValue(key, out T? cached) && cached != null)
            {
                return cached;
            }

            var value = await factory();
            _cache.Set(key, value, Ttl);
            return value;
        }
    }
}
