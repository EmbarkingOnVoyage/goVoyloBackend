using GoVoylo.Application.Features.Flights.Dtos;

namespace GoVoylo.Application.Interfaces
{
    // Renders a booking's e-ticket / invoice as a PDF.
    public interface IETicketPdfGenerator
    {
        byte[] Generate(ETicketDocumentDto ticket);
    }
}
