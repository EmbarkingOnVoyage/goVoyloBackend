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

        Task<SupplierFareRulesResultDto> GetFareRulesAsync(
            string searchKey, string flightKey, string fareId, CancellationToken cancellationToken);
    }
}
