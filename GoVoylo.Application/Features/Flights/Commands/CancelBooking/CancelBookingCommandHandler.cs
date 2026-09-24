using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, CancelBookingResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;

        public CancelBookingCommandHandler(IFlightSupplierClient supplierClient)
        {
            _supplierClient = supplierClient;
        }

        public async Task<CancelBookingResponseDto> Handle(
            CancelBookingCommand request, CancellationToken cancellationToken)
        {
            await _supplierClient.CancelBookingAsync(
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
