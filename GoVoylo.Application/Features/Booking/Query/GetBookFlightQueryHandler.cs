using GoVoylo.Application.Features.Booking.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;


namespace GoVoylo.Application.Features.Booking.Query
{
    public class GetBookFlightQueryHandler
    : IRequestHandler<GetBookFlightQuery, GetBookFlightResponseDto>
    {
        private readonly IBookFlightRepository _bookingRepository;

        //DI(When .NET creates this handler, it automatically provides an implementation of IBookFlightRepository.)
        public GetBookFlightQueryHandler(
            IBookFlightRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<GetBookFlightResponseDto> Handle(
            GetBookFlightQuery query,
            CancellationToken cancellationToken)
        {
            //Finds bookings with the referenceId 
            var booking = await _bookingRepository.GetByBookingReferenceAsync(
                query.BookingReference,
                cancellationToken);

            if (booking == null)
            {
                throw new Exception($"Flight booking with reference '{query.BookingReference}' was not found.");
            }
            return new GetBookFlightResponseDto(
                booking.FlightBookingId,
                booking.FlightBookingReference,
                booking.FlightNumber,
                booking.PassengerName
            );
        }
    }
}