using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    public class TravelerSpecialAssistance : BaseEntity
    {
        public Guid SavedTravelerId { get; private set; }
        public string SsrCode { get; private set; } = null!; // IATA SSR code: WCHR, MEDA, INFT, ...
        public string? Notes { get; private set; }

        public TravelerSpecialAssistance(Guid savedTravelerId, string ssrCode, string? notes)
        {
            SavedTravelerId = savedTravelerId;
            SsrCode = ssrCode;
            Notes = notes;
        }

        // Required by EF Core
        private TravelerSpecialAssistance()
        {
        }
    }
}
