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

        Task<SupplierTempBookingResultDto> CreateTempBookingAsync(
            SupplierTempBookingRequestDto request, CancellationToken cancellationToken);

        // Deliberately Block_Ticket only (Ticketing_Type "0" — a reversible hold,
        // cancellable via Air_ReleasePNR), never Book_Ticket ("1"). Book_Ticket
        // requires an Add_Payment call first, which debits GoVoylo's real Flyshop
        // agency wallet balance and produces an essentially final airline PNR —
        // a materially bigger, real-money decision that hasn't been authorized.
        // See FLIGHT_ANCILLARIES_SCOPE.MD in the repo root for the fuller writeup.
        Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken);
    }
}
