using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.GetMyTripBookings;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Application.Features.Flights.Queries.GetTripBookingDetails
{
    public class GetTripBookingDetailsQueryHandler
        : IRequestHandler<GetTripBookingDetailsQuery, TripBookingDetailsDto>
    {
        // Failed bookings were never held or ticketed, so the supplier has nothing to return.
        private const string StatusFailed = "22";

        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly ILogger<GetTripBookingDetailsQueryHandler> _logger;

        public GetTripBookingDetailsQueryHandler(
            ITripBookingRepository tripBookingRepository,
            IFlightSupplierClientResolver supplierClientResolver,
            ILogger<GetTripBookingDetailsQueryHandler> logger)
        {
            _tripBookingRepository = tripBookingRepository;
            _supplierClientResolver = supplierClientResolver;
            _logger = logger;
        }

        public async Task<TripBookingDetailsDto> Handle(
            GetTripBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            var booking = await _tripBookingRepository.GetByIdAsync(request.TripBookingId, cancellationToken)
                ?? throw new NotFoundException("Booking not found.");

            if (booking.UserId != request.UserId)
            {
                throw new ForbiddenException("not_your_booking", "This booking does not belong to you.");
            }

            var bookingDto = TripBookingMapping.ToDto(booking);

            if (booking.StatusId != StatusFailed)
            {
                try
                {
                    var details = await _supplierClientResolver
                        .Resolve(booking.SupplierCode)
                        .GetBookingDetailsAsync(booking.BookingRefNo, booking.AirlinePnr, cancellationToken);

                    return new TripBookingDetailsDto(
                        bookingDto,
                        true,
                        details.Segments
                            .Select(s => new TripBookingSegmentDto(
                                s.TripIndex,
                                s.Origin,
                                s.Destination,
                                s.AirlineCode,
                                s.AirlineName,
                                s.FlightNumber,
                                s.DepartureDateTime,
                                s.ArrivalDateTime,
                                s.DurationMinutes))
                            .ToList(),
                        details.Passengers
                            .Select(p => new TripBookingPassengerDto(p.Title, p.FirstName, p.LastName, p.PaxType))
                            .ToList(),
                        details.BaseFare,
                        details.TaxesAndFees,
                        booking.PayableAmount);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex,
                        "Supplier booking details failed for trip booking {TripBookingId}; using local data.",
                        booking.Id);
                }
            }

            return new TripBookingDetailsDto(
                bookingDto,
                false,
                Array.Empty<TripBookingSegmentDto>(),
                Array.Empty<TripBookingPassengerDto>(),
                null,
                null,
                booking.PayableAmount);
        }
    }
}
