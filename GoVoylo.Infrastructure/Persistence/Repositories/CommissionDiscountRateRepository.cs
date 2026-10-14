using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    // Read on every search and booking, so cached; a new row in the database takes
    // effect within CacheTtl.
    public class CommissionDiscountRateRepository : ICommissionDiscountRateRepository
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
        private const string RatesKey = "commission-discount:rates";

        // With no rows at all, prices stay as they were before discounts existed
        // (the whole commission given back) rather than rising by surprise.
        private const decimal DefaultPercent = 100m;

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public CommissionDiscountRateRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<decimal> GetPercentAtAsync(DateTime utc, CancellationToken cancellationToken)
        {
            var rates = await _cache.GetOrCreateAsync(RatesKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                IReadOnlyList<CommissionDiscountRate> all = await _context.CommissionDiscountRates
                    .AsNoTracking()
                    .OrderByDescending(x => x.EffectiveFrom)
                    .ToListAsync(cancellationToken);
                return all;
            }) ?? Array.Empty<CommissionDiscountRate>();

            return rates.FirstOrDefault(r => r.EffectiveFrom <= utc)?.Percent ?? DefaultPercent;
        }
    }
}
