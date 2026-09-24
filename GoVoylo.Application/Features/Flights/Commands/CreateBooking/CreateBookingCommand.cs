using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CreateBooking
{
    public record CreateBookingCommand(
        // Taken from the authenticated caller (ICurrentUserService), never from the
        // request body — see FlightsController.CreateBooking. Used to look up the
        // account's registered email for the booking-confirmation email, which is
        // deliberately separate from PassengerEmail (the trip's contact email,
        // which may belong to someone else being booked for).
        Guid UserId,
        IReadOnlyList<BookingLegRequestDto> Legs,
        IReadOnlyList<BookingTravelerRequestDto> Travelers,
        string PassengerMobile,
        string PassengerEmail) : IRequest<CreateBookingResponseDto>;
}
