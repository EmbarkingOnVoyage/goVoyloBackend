using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetMyTripBookings
{
    public record GetMyTripBookingsQuery(Guid UserId) : IRequest<IReadOnlyList<TripBookingDto>>;
}
