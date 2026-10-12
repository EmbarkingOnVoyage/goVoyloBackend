using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.ReleaseHold
{
    // Gives up a hold whose checkout didn't end in a payment (Razorpay closed,
    // payment failed or unverified): releases it with the supplier when there's
    // an airline PNR (Air_ReleasePNR for Flyshop — Air_TicketCancellation rejects
    // un-ticketed holds with Err007, see FLIGHT_ANCILLARIES_SCOPE.MD) and marks
    // the TripBooking Released so My Trips stops showing it as on hold. Holds with
    // no PNR (Flyshop direct temp bookings, Tripjack fares that can't be held)
    // have nothing to release with the supplier and are just marked Released.
    public record ReleaseHoldCommand(
        Guid UserId,
        string BookingRefNo,
        string? AirlinePnr) : IRequest<ReleaseHoldResponseDto>;

    public record ReleaseHoldRequest(string BookingRefNo, string? AirlinePnr);
}
