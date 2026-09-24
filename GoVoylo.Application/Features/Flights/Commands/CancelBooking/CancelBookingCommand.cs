using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CancelBooking
{
    // Mirrors Air_TicketCancellation's own request shape almost 1:1 (RefNo,
    // Airline_PNR, CancellationType, CancelCode, ReqRemarks, per-segment
    // FlightId/PassengerId/SegmentId) rather than looking these up from a
    // persisted booking record, since bookings aren't persisted anywhere yet
    // (see FLIGHT_ANCILLARIES_SCOPE.MD) — the caller supplies the identifiers
    // returned earlier from CreateBookingCommand's response.
    public record CancelBookingCommand(
        string RefNo,
        string AirlinePnr,
        int CancellationType,
        string CancelCode,
        string ReqRemarks,
        IReadOnlyList<CancelBookingSegmentDto> Segments) : IRequest<CancelBookingResponseDto>;
}
