using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Interfaces
{
    // Turns a paid-for booking (a held fare, or a deferred non-holdable one) into a
    // ticketed one with its supplier, once its Razorpay payment is verified.
    public interface ITripBookingTicketingService
    {
        Task TicketAsync(TripBooking booking, string clientRefNo, CancellationToken cancellationToken);
    }
}
