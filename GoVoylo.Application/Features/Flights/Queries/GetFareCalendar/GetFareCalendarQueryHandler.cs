using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareCalendar
{
    public class GetFareCalendarQueryHandler
        : IRequestHandler<GetFareCalendarQuery, FareCalendarResponseDto>
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;

        public GetFareCalendarQueryHandler(IFlightSupplierClientResolver supplierClientResolver)
        {
            _supplierClientResolver = supplierClientResolver;
        }

        public async Task<FareCalendarResponseDto> Handle(
            GetFareCalendarQuery request, CancellationToken cancellationToken)
        {
            // Flyshop-only: Tripjack's own API docs don't offer a low-fare-calendar
            // endpoint at all, so there's no second supplier to resolve here.
            var supplierClient = _supplierClientResolver.Resolve(FlightSupplierCodes.Flyshop);

            var result = await supplierClient.GetLowFareCalendarAsync(
                new SupplierLowFareRequestDto(request.Origin, request.Destination, request.Month, request.Year),
                cancellationToken);

            var days = result.Days
                .Select(d => new FareCalendarDayDto(d.TravelDate, d.Amount, d.CurrencyCode))
                .ToList();

            return new FareCalendarResponseDto(days);
        }
    }
}
