using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.ReleaseHold
{
    public class ReleaseHoldCommandHandler : IRequestHandler<ReleaseHoldCommand, ReleaseHoldResponseDto>
    {
        private const string StatusBlocked = "33";

        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly ITripBookingRepository _tripBookingRepository;

        public ReleaseHoldCommandHandler(
            IFlightSupplierClientResolver supplierClientResolver,
            ITripBookingRepository tripBookingRepository)
        {
            _supplierClientResolver = supplierClientResolver;
            _tripBookingRepository = tripBookingRepository;
        }

        public async Task<ReleaseHoldResponseDto> Handle(
            ReleaseHoldCommand request, CancellationToken cancellationToken)
        {
            var booking = await _tripBookingRepository.GetByBookingRefNoAsync(request.BookingRefNo, cancellationToken);

            if (booking == null)
            {
                // CreateBookingCommandHandler's persist is best-effort, so a hold can
                // exist with the supplier without a TripBooking. Every such hold
                // predates multi-supplier support, hence Flyshop.
                if (!string.IsNullOrWhiteSpace(request.AirlinePnr))
                {
                    await _supplierClientResolver.Resolve(FlightSupplierCodes.Flyshop).ReleaseHoldAsync(
                        new SupplierReleaseHoldRequestDto(request.BookingRefNo, request.AirlinePnr),
                        cancellationToken);
                }
                return new ReleaseHoldResponseDto(true);
            }

            if (booking.UserId != request.UserId)
            {
                throw new ForbiddenException("not_your_booking", "This booking does not belong to you.");
            }

            // Already released, ticketed, ticketing or failed: nothing to give up,
            // and a paid booking must never be released from here.
            if (booking.LocalStatus != TripBooking.StatusActive
                || booking.StatusId != StatusBlocked
                || booking.AmountPaid.HasValue)
            {
                return new ReleaseHoldResponseDto(true);
            }

            if (!string.IsNullOrWhiteSpace(booking.AirlinePnr))
            {
                // Throws (via EnsureSuccess) when the supplier refuses, leaving the
                // booking active so the customer can still release it from My Trips.
                await _supplierClientResolver.Resolve(booking.SupplierCode).ReleaseHoldAsync(
                    new SupplierReleaseHoldRequestDto(booking.BookingRefNo, booking.AirlinePnr),
                    cancellationToken);
            }

            booking.MarkReleased();
            await _tripBookingRepository.UpdateAsync(booking, cancellationToken);

            return new ReleaseHoldResponseDto(true);
        }
    }
}
