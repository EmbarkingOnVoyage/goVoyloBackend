using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    // Rules change rarely and are read on every search and booking, so they're
    // cached; an edit in the database takes effect within CacheTtl.
    public class ConvenienceFeeRuleRepository : IConvenienceFeeRuleRepository
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
        private const string TripRatesKey = "convenience-fee:trip-rates";
        private const string PaxBandsKey = "convenience-fee:pax-bands";

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public ConvenienceFeeRuleRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IReadOnlyList<ConvenienceFeeTripRate>> GetActiveTripRatesAsync(CancellationToken cancellationToken) =>
            await _cache.GetOrCreateAsync(TripRatesKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                IReadOnlyList<ConvenienceFeeTripRate> rates = await _context.ConvenienceFeeTripRates
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .ToListAsync(cancellationToken);
                return rates;
            }) ?? Array.Empty<ConvenienceFeeTripRate>();

        public async Task<IReadOnlyList<ConvenienceFeePaxBand>> GetActivePaxBandsAsync(CancellationToken cancellationToken) =>
            await _cache.GetOrCreateAsync(PaxBandsKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                IReadOnlyList<ConvenienceFeePaxBand> bands = await _context.ConvenienceFeePaxBands
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.MinPax)
                    .ToListAsync(cancellationToken);
                return bands;
            }) ?? Array.Empty<ConvenienceFeePaxBand>();
    }
}
