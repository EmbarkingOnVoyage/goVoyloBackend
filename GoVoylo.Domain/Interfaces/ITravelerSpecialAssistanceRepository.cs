using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITravelerSpecialAssistanceRepository
    {
        Task<IReadOnlyList<TravelerSpecialAssistance>> GetByTravelerIdAsync(Guid savedTravelerId);

        // Preferences are updated as a full replace — the client sends the complete
        // desired SSR-code list each time, matching the merged /preferences endpoint.
        Task ReplaceAllAsync(Guid savedTravelerId, IEnumerable<TravelerSpecialAssistance> items);
    }
}
