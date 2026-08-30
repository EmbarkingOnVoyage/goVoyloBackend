using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // One row per (user, airport) — recency is tracked by bumping SearchedAt on repeat
    // searches rather than accumulating duplicate rows.
    public class RecentAirportSearch : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string IataCode { get; private set; } = null!;
        public DateTime SearchedAt { get; private set; }

        public RecentAirportSearch(Guid userId, string iataCode)
        {
            UserId = userId;
            IataCode = iataCode.ToUpperInvariant();
            SearchedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private RecentAirportSearch()
        {
        }

        public void Touch()
        {
            SearchedAt = DateTime.UtcNow;
        }
    }
}
