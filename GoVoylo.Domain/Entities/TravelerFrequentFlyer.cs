using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerFrequentFlyer : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public string AirlineCode { get; private set; } = null!;
        public byte[] MembershipNumberEncrypted { get; private set; } = null!;

        public TravelerFrequentFlyer(Guid savedTravelerId, string airlineCode, byte[] membershipNumberEncrypted)
        {
            SavedTravelerId = savedTravelerId;
            AirlineCode = airlineCode;
            MembershipNumberEncrypted = membershipNumberEncrypted;
        }

        // Required by EF Core
        private TravelerFrequentFlyer()
        {
        }
    }
}
