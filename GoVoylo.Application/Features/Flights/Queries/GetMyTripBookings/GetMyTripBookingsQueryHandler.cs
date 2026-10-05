using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetMyTripBookings
{
    public class GetMyTripBookingsQueryHandler
        : IRequestHandler<GetMyTripBookingsQuery, IReadOnlyList<TripBookingDto>>
    {
        private readonly ITripBookingRepository _tripBookingRepository;

        public GetMyTripBookingsQueryHandler(ITripBookingRepository tripBookingRepository)
        {
            _tripBookingRepository = tripBookingRepository;
        }

        public async Task<IReadOnlyList<TripBookingDto>> Handle(
            GetMyTripBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _tripBookingRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            return bookings.Select(TripBookingMapping.ToDto).ToList();
        }
    }
}
