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

        // Places a reversible Block_Ticket hold (Ticketing_Type "0"), cancellable via
        // Air_ReleasePNR — not a final purchase. See FLIGHT_ANCILLARIES_SCOPE.MD for
        // the fuller writeup of the hold → pay → ticket flow.
        Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken);

        // Debits GoVoylo's real Flyshop agency wallet balance against an existing
        // Block_Ticket hold's Booking_RefNo — a real settlement charge, not a sandbox
        // echo. Must succeed before BookTicketAsync will; see AddPaymentRequestWire's
        // own doc comment for the wire contract (sourced from Flyshop's "Client 2.6
        // Air" Postman collection, AddPayment endpoint).
        Task<SupplierPaymentResultDto> AddPaymentAsync(
            string bookingRefNo, string clientRefNo, CancellationToken cancellationToken);

        // Converts an already-paid-for Block_Ticket hold into a real, essentially
        // final airline PNR (Ticketing_Type "1"). Only call this after AddPaymentAsync
        // has succeeded for the same bookingRefNo — Flyshop's own Air_Ticketing
        // rejects Book_Ticket against a hold with no registered payment.
        Task<SupplierTicketingResultDto> BookTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken);

        Task<SupplierFareRuleResultDto> GetFareRulesAsync(
            SupplierFareRuleRequestDto request, CancellationToken cancellationToken);

        Task CancelBookingAsync(
            SupplierCancellationRequestDto request, CancellationToken cancellationToken);

        // Releases a Block_Ticket hold that was never converted to a real ticket.
        // See CancelBookingAsync's own doc comment / FLIGHT_ANCILLARIES_SCOPE.MD for
        // why this is a separate operation from cancellation: Air_TicketCancellation
        // rejects an un-ticketed hold outright (confirmed against live UAT).
        Task ReleaseHoldAsync(
            SupplierReleaseHoldRequestDto request, CancellationToken cancellationToken);
    }
}
