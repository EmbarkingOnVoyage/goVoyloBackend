using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CancelTripBooking
{
    // Cancels or releases a booking from "My Trips" using only its internal id — the
    // caller never has to resupply Flyshop's own RefNo/Airline_PNR/FlightId/PaxIds,
    // since those were captured once at booking time (see TripBooking). UserId comes
    // from the authenticated caller (ICurrentUserService), never the request body, so
    // one user can't cancel another user's booking.
    // CancellationType/CancelCode are optional — omitted (null) for the ordinary "My
    // Trips" cancel button, which keeps the handler's own customer-initiated default
    // (0/"015"); a caller that needs a specific Air_TicketCancellation type (e.g.
    // 1-Full Refund, 2-No Show) can supply it directly. Only meaningful for the
    // already-ticketed branch — a Block_Ticket hold is always released via
    // Air_ReleasePNR regardless of what's passed here.
    // LegIndex is also optional — omitted (null) cancels every leg (the ordinary
    // behavior, and the only option for a oneway booking). Supplying a specific
    // TripBookingLeg.LegIndex cancels only that leg (e.g. the return leg of a
    // roundtrip) and leaves the booking Active as long as another leg still stands.
    public record CancelTripBookingCommand(
        Guid TripBookingId,
        Guid UserId,
        int? CancellationType = null,
        string? CancelCode = null,
        int? LegIndex = null) : IRequest<CancelTripBookingResponseDto>;
}
