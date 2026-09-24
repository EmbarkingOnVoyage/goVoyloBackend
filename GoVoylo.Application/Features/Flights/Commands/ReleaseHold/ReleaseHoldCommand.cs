using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.ReleaseHold
{
    // Releases a Block_Ticket hold (Ticketing_Type "0") that was never converted to a
    // real ticket, via Air_ReleasePNR. Separate from CancelBookingCommand because
    // Flyshop rejects Air_TicketCancellation outright for an un-ticketed hold (confirmed
    // against live UAT: Err007 "Please check your RefNo Status") — see
    // FLIGHT_ANCILLARIES_SCOPE.MD. The caller supplies the identifiers returned earlier
    // from CreateBookingCommand's response, since bookings aren't persisted anywhere yet.
    public record ReleaseHoldCommand(
        string BookingRefNo,
        string AirlinePnr) : IRequest<ReleaseHoldResponseDto>;
}
