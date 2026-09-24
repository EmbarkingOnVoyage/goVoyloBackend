using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.ReleaseHold
{
    public class ReleaseHoldCommandHandler : IRequestHandler<ReleaseHoldCommand, ReleaseHoldResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;

        public ReleaseHoldCommandHandler(IFlightSupplierClient supplierClient)
        {
            _supplierClient = supplierClient;
        }

        public async Task<ReleaseHoldResponseDto> Handle(
            ReleaseHoldCommand request, CancellationToken cancellationToken)
        {
            await _supplierClient.ReleaseHoldAsync(
                new SupplierReleaseHoldRequestDto(request.BookingRefNo, request.AirlinePnr),
                cancellationToken);

            // ReleaseHoldAsync throws (via EnsureSuccess) on a non-"0000" Flyshop
            // response, so reaching here means the release request succeeded.
            return new ReleaseHoldResponseDto(true);
        }
    }
}
