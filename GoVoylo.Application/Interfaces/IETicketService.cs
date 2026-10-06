using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Interfaces
{
    // Builds a ticketed booking's e-ticket PDF and emails it to the customer.
    public interface IETicketService
    {
        Task SendAsync(TripBooking booking, CancellationToken cancellationToken);

        // The same PDF, for the app's Download Ticket button.
        Task<ETicketFile> BuildPdfAsync(TripBooking booking, CancellationToken cancellationToken);
    }

    public record ETicketFile(byte[] Content, string FileName, string RouteSummary);
}
