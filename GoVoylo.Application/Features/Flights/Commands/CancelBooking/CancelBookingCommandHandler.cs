using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CancelBooking
{
    // No live caller today (the app's own cancel flow goes through
    // CancelTripBookingCommand / "My Trips" instead, which resolves its supplier from
    // the stored TripBooking) — this raw RefNo+PNR endpoint predates that and has no
    // booking to look a supplier up from, so it's pinned to Flyshop rather than
    // redesigned for a caller that doesn't exist yet.
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, CancelBookingResponseDto>
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;

        public CancelBookingCommandHandler(IFlightSupplierClientResolver supplierClientResolver)
        {
            _supplierClientResolver = supplierClientResolver;
        }

        public async Task<CancelBookingResponseDto> Handle(
            CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var supplierClient = _supplierClientResolver.Resolve(FlightSupplierCodes.Flyshop);

            await supplierClient.CancelBookingAsync(
                new SupplierCancellationRequestDto(
                    request.RefNo,
                    request.AirlinePnr,
                    request.CancellationType,
                    request.CancelCode,
                    request.ReqRemarks,
                    request.Segments
                        .Select(s => new SupplierCancelSegmentDto(s.FlightId, s.PassengerId, s.SegmentId))
                        .ToList()),
                cancellationToken);

            // CancelBookingAsync throws (via EnsureSuccess) on a non-"0000" Flyshop
            // response, so reaching here means the cancellation request succeeded.
            return new CancelBookingResponseDto(true);
        }
    }
}
