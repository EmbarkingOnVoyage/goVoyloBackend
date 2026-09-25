using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ITripBookingRepository
    {
        Task AddAsync(TripBooking booking, CancellationToken cancellationToken);

        Task<TripBooking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<TripBooking?> GetByBookingRefNoAsync(string bookingRefNo, CancellationToken cancellationToken);

        Task<IReadOnlyList<TripBooking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task UpdateAsync(TripBooking booking, CancellationToken cancellationToken);
    }
}
