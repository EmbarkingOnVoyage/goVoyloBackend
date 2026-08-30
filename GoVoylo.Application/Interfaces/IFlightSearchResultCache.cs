using GoVoylo.Application.Features.Flights.Dtos;

namespace GoVoylo.Application.Interfaces
{
    // Holds the full offer list from one search, keyed by SearchId, so filter/sort/summary
    // calls can operate on it without re-calling the supplier. Same lifetime as an offer's
    // own session (see IFlightSearchSessionStore) — both expire when the fare hold would.
    public interface IFlightSearchResultCache
    {
        Task SaveAsync(Guid searchId, IReadOnlyList<FlightOfferDto> offers, CancellationToken cancellationToken);

        Task<IReadOnlyList<FlightOfferDto>?> GetAsync(Guid searchId, CancellationToken cancellationToken);
    }
}
