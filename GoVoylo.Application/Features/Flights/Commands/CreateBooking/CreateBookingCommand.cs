using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CreateBooking
{
    public record CreateBookingCommand(
        IReadOnlyList<BookingLegRequestDto> Legs,
        IReadOnlyList<BookingTravelerRequestDto> Travelers,
        string PassengerMobile,
        string PassengerEmail) : IRequest<CreateBookingResponseDto>;
}
