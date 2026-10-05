using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetTripBookingDetails
{
    // UserId comes from the authenticated caller, so one user can't read another's booking.
    public record GetTripBookingDetailsQuery(Guid TripBookingId, Guid UserId) : IRequest<TripBookingDetailsDto>;
}
