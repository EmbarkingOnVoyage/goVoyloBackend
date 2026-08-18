using GoVoylo.Application.Features.Flights.Dtos;

namespace GoVoylo.Application.Interfaces
{
    public interface IFlightSearchSessionStore
    {
        Task<Guid> SaveAsync(FlightOfferSession session, CancellationToken cancellationToken);

        Task<FlightOfferSession?> GetAsync(Guid offerId, CancellationToken cancellationToken);

        Task UpdateAsync(Guid offerId, FlightOfferSession session, CancellationToken cancellationToken);
    }
}
