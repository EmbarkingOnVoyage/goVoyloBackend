using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.ReleaseHold
{
    public class ReleaseHoldCommandHandler : IRequestHandler<ReleaseHoldCommand, ReleaseHoldResponseDto>
    {
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
            // Called when the Pay Now checkout flow backs out before a TripBooking
            // necessarily exists yet (e.g. the user cancels Razorpay before
            // CreateBookingCommandHandler's own best-effort persist even ran) — fall
            // back to Flyshop in that case, since every hold releasable this way was
            // created before multi-supplier support existed.
            var booking = await _tripBookingRepository.GetByBookingRefNoAsync(request.BookingRefNo, cancellationToken);
            var supplierCode = booking?.SupplierCode ?? FlightSupplierCodes.Flyshop;
            var supplierClient = _supplierClientResolver.Resolve(supplierCode);

            await supplierClient.ReleaseHoldAsync(
                new SupplierReleaseHoldRequestDto(request.BookingRefNo, request.AirlinePnr),
                cancellationToken);

            // ReleaseHoldAsync throws (via EnsureSuccess) on a non-"0000" Flyshop
            // response, so reaching here means the release request succeeded.
            return new ReleaseHoldResponseDto(true);
        }
    }
}
