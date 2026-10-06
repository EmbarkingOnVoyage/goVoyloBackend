using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Interfaces
{
    // Builds a ticketed booking's e-ticket PDF and emails it to the customer.
    public interface IETicketService
    {
        Task SendAsync(TripBooking booking, CancellationToken cancellationToken);
    }
}
