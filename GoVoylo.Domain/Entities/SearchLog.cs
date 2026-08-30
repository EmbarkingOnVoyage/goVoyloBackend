using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // One row per searched origin-destination leg. UserId is optional so guest
    // searches still count toward popular-routes aggregation.
    public class SearchLog : BaseEntity
    {
        public Guid? UserId { get; private set; }
        public string Origin { get; private set; } = null!;
        public string Destination { get; private set; } = null!;
        public DateTime TravelDate { get; private set; }
        public string TripType { get; private set; } = null!;
        public string CabinClass { get; private set; } = null!;
        public DateTime SearchedAt { get; private set; }

        public SearchLog(
            Guid? userId, string origin, string destination, DateTime travelDate,
            string tripType, string cabinClass)
        {
            UserId = userId;
            Origin = origin.ToUpperInvariant();
            Destination = destination.ToUpperInvariant();
            TravelDate = travelDate;
            TripType = tripType;
            CabinClass = cabinClass;
            SearchedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private SearchLog()
        {
        }
    }
}
