using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareCalendar
{
    public class GetFareCalendarQueryHandler
        : IRequestHandler<GetFareCalendarQuery, FareCalendarResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;

        public GetFareCalendarQueryHandler(IFlightSupplierClient supplierClient)
        {
            _supplierClient = supplierClient;
        }

        public async Task<FareCalendarResponseDto> Handle(
            GetFareCalendarQuery request, CancellationToken cancellationToken)
        {
            var result = await _supplierClient.GetLowFareCalendarAsync(
                new SupplierLowFareRequestDto(request.Origin, request.Destination, request.Month, request.Year),
                cancellationToken);

            var days = result.Days
                .Select(d => new FareCalendarDayDto(d.TravelDate, d.Amount, d.CurrencyCode))
                .ToList();

            return new FareCalendarResponseDto(days);
        }
    }
}
