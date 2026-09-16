using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.Caching
{
    public class InMemoryFlightSearchSessionStore : IFlightSearchSessionStore
    {
        private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;

        public InMemoryFlightSearchSessionStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<Guid> SaveAsync(FlightOfferSession session, CancellationToken cancellationToken)
        {
            var offerId = Guid.NewGuid();
            _cache.Set(CacheKey(offerId), session, SessionTtl);
            return Task.FromResult(offerId);
        }

        public Task<FlightOfferSession?> GetAsync(Guid offerId, CancellationToken cancellationToken)
        {
            _cache.TryGetValue(CacheKey(offerId), out FlightOfferSession? session);
            return Task.FromResult(session);
        }

        public Task UpdateAsync(Guid offerId, FlightOfferSession session, CancellationToken cancellationToken)
        {
            _cache.Set(CacheKey(offerId), session, SessionTtl);
            return Task.CompletedTask;
        }

        private static string CacheKey(Guid offerId) => $"flight-offer:{offerId}";
    }
}
