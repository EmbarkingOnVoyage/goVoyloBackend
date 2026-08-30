using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.Caching
{
    public class InMemoryFlightSearchResultCache : IFlightSearchResultCache
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;

        public InMemoryFlightSearchResultCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SaveAsync(Guid searchId, IReadOnlyList<FlightOfferDto> offers, CancellationToken cancellationToken)
        {
            _cache.Set(CacheKey(searchId), offers, Ttl);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<FlightOfferDto>?> GetAsync(Guid searchId, CancellationToken cancellationToken)
        {
            _cache.TryGetValue(CacheKey(searchId), out IReadOnlyList<FlightOfferDto>? offers);
            return Task.FromResult(offers);
        }

        private static string CacheKey(Guid searchId) => $"flight-search-results:{searchId}";
    }
}
