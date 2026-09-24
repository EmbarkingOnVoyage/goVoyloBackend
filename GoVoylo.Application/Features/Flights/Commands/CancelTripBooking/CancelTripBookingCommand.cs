using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CancelTripBooking
{
    // Cancels or releases a booking from "My Trips" using only its internal id — the
    // caller never has to resupply Flyshop's own RefNo/Airline_PNR/FlightId/PaxIds,
    // since those were captured once at booking time (see TripBooking). UserId comes
    // from the authenticated caller (ICurrentUserService), never the request body, so
    // one user can't cancel another user's booking.
    public record CancelTripBookingCommand(
        Guid TripBookingId,
        Guid UserId) : IRequest<CancelTripBookingResponseDto>;
}
