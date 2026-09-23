using GoVoylo.Application.Features.Flights.Dtos;

namespace GoVoylo.Application.Interfaces
{
    public interface IFlightSupplierClient
    {
        string SupplierCode { get; }

        Task<SupplierFlightSearchResultDto> SearchAsync(
            FlightSearchRequestDto request, CancellationToken cancellationToken);

        Task<SupplierRepriceResultDto> RepriceAsync(
            SupplierRepriceRequestDto request, CancellationToken cancellationToken);

        Task<SupplierLowFareResultDto> GetLowFareCalendarAsync(
            SupplierLowFareRequestDto request, CancellationToken cancellationToken);

        Task<SupplierAncillaryResultDto> GetAncillariesAsync(
            SupplierAncillaryRequestDto request, CancellationToken cancellationToken);

        Task<SupplierSeatMapResultDto> GetSeatMapAsync(
            SupplierSeatMapRequestDto request, CancellationToken cancellationToken);
    }
}
