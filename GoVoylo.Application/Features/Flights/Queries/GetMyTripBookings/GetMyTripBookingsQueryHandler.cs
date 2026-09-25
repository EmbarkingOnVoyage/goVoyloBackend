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

            return bookings
                .Select(b => new TripBookingDto(
                    b.Id,
                    b.BookingRefNo,
                    b.AirlinePnr,
                    b.CrsPnr,
                    b.StatusId,
                    b.LocalStatus,
                    b.TotalAmount,
                    b.CurrencyCode,
                    b.PassengerNames,
                    b.CreatedAt,
                    b.CancellationType,
                    b.CancelCode,
                    b.Legs
                        .Select(l => new TripBookingLegDto(
                            l.LegIndex,
                            l.Origin,
                            l.Destination,
                            l.TravelDate,
                            l.AirlineCode,
                            l.AirlineName,
                            l.FlightNumber,
                            l.IsCancelled))
                        .ToList()))
                .ToList();
        }
    }
}
