using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareCalendar
{
    public class GetFareCalendarQueryHandler
        : IRequestHandler<GetFareCalendarQuery, FareCalendarResponseDto>
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IConfiguration? _configuration;

        public GetFareCalendarQueryHandler(
            IFlightSupplierClientResolver supplierClientResolver, IConfiguration? configuration = null)
        {
            _supplierClientResolver = supplierClientResolver;
            _configuration = configuration;
        }

        public async Task<FareCalendarResponseDto> Handle(
            GetFareCalendarQuery request, CancellationToken cancellationToken)
        {
            // Flyshop-only: Tripjack's own API docs don't offer a low-fare-calendar
            // endpoint at all, so there's no second supplier to resolve here.
            // With Flyshop switched off its fares can't be booked, so no prices are shown.
            if (_configuration != null && !FlightSupplierSwitch.IsEnabled(_configuration, FlightSupplierCodes.Flyshop))
            {
                return new FareCalendarResponseDto(Array.Empty<FareCalendarDayDto>());
            }

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
