using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetETicket
{
    // The booking's e-ticket PDF — the same one emailed after ticketing — for the
    // app's Download Ticket button. UserId comes from the authenticated caller.
    public record GetETicketQuery(Guid TripBookingId, Guid UserId) : IRequest<ETicketFile>;
}
