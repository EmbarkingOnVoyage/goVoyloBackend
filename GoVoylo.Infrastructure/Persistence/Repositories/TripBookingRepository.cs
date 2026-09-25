using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class TripBookingRepository : ITripBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public TripBookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TripBooking booking, CancellationToken cancellationToken)
        {
            await _context.TripBookings.AddAsync(booking, cancellationToken);
            await _context.TripBookingLegs.AddRangeAsync(booking.Legs, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<TripBooking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var booking = await _context.TripBookings
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (booking == null)
            {
                return null;
            }

            var legs = await _context.TripBookingLegs
                .Where(x => x.TripBookingId == booking.Id)
                .OrderBy(x => x.LegIndex)
                .ToListAsync(cancellationToken);

            booking.LoadLegs(legs);
            return booking;
        }

        public async Task<TripBooking?> GetByBookingRefNoAsync(string bookingRefNo, CancellationToken cancellationToken)
        {
            var booking = await _context.TripBookings
                .FirstOrDefaultAsync(x => x.BookingRefNo == bookingRefNo, cancellationToken);

            if (booking == null)
            {
                return null;
            }

            var legs = await _context.TripBookingLegs
                .Where(x => x.TripBookingId == booking.Id)
                .OrderBy(x => x.LegIndex)
                .ToListAsync(cancellationToken);

            booking.LoadLegs(legs);
            return booking;
        }

        public async Task<IReadOnlyList<TripBooking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var bookings = await _context.TripBookings
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var bookingIds = bookings.Select(x => x.Id).ToList();

            var legs = await _context.TripBookingLegs
                .Where(x => bookingIds.Contains(x.TripBookingId))
                .OrderBy(x => x.LegIndex)
                .ToListAsync(cancellationToken);

            var legsByBookingId = legs.ToLookup(x => x.TripBookingId);

            foreach (var booking in bookings)
            {
                booking.LoadLegs(legsByBookingId[booking.Id]);
            }

            return bookings;
        }

        public async Task UpdateAsync(TripBooking booking, CancellationToken cancellationToken)
        {
            _context.TripBookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
