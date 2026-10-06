using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Application.Features.Payments.Services
{
    public class TripBookingTicketingService : ITripBookingTicketingService
    {
        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IEncryptionService _encryptionService;
        private readonly IETicketService _eTicketService;
        private readonly ILogger<TripBookingTicketingService> _logger;

        // Air_Ticketing / Tripjack status for an issued ticket. 44 (paid, still
        // ticketing) has no ticket to send yet.
        private const string StatusTicketed = "11";

        public TripBookingTicketingService(
            ITripBookingRepository tripBookingRepository,
            IFlightSupplierClientResolver supplierClientResolver,
            IEncryptionService encryptionService,
            IETicketService eTicketService,
            ILogger<TripBookingTicketingService> logger)
        {
            _tripBookingRepository = tripBookingRepository;
            _supplierClientResolver = supplierClientResolver;
            _encryptionService = encryptionService;
            _eTicketService = eTicketService;
            _logger = logger;
        }

        public async Task TicketAsync(TripBooking booking, string clientRefNo, CancellationToken cancellationToken)
        {
            // Deliberately not swallowed, unlike the best-effort email/persist paths
            // in CreateBookingCommandHandler: the customer has already paid by this
            // point, so a supplier failure here must surface as a real error (needing
            // manual follow-up — refund or retry) rather than being swallowed into a
            // false "booking confirmed" response.
            var supplierClient = _supplierClientResolver.Resolve(booking.SupplierCode);

            // Set only for a fare the supplier couldn't hold — see
            // TripBooking.DeferredSupplierPayloadEncrypted.
            var deferredBookPayload = booking.DeferredSupplierPayloadEncrypted is { } encrypted
                ? _encryptionService.Decrypt(encrypted)
                : null;

            SupplierTicketingResultDto ticket;
            try
            {
                await supplierClient.AddPaymentAsync(
                    booking.BookingRefNo, clientRefNo, deferredBookPayload, cancellationToken);

                ticket = await supplierClient.BookTicketAsync(booking.BookingRefNo, deferredBookPayload, cancellationToken);
            }
            catch
            {
                // Paid but not ticketed — recorded as failed so it's visible for a
                // manual refund/retry, then surfaced to the caller.
                booking.MarkTicketingFailed();
                await _tripBookingRepository.UpdateAsync(booking, cancellationToken);
                throw;
            }

            booking.MarkTicketed(ticket.StatusId, ticket.AirlinePnr, ticket.CrsPnr, ticket.RecordLocator);

            // A roundtrip's two legs can come back with different Airline_PNR values
            // (Air_Ticketing returns one AirlinePNRDetails entry per Flight_Id) — the
            // booking-level fields above only ever hold the first leg's values, so each
            // leg needs its own PNR recorded for a later leg-specific cancellation to
            // send the right one. Matched by FlightId since that's the one identifier
            // both sides share.
            var legResultsByFlightId = ticket.Legs
                .GroupBy(l => l.FlightId)
                .ToDictionary(g => g.Key, g => g.First());
            foreach (var leg in booking.Legs)
            {
                if (legResultsByFlightId.TryGetValue(leg.FlightId, out var legResult))
                {
                    leg.MarkTicketed(legResult.AirlinePnr, legResult.CrsPnr, legResult.RecordLocator);
                }
            }

            await _tripBookingRepository.UpdateAsync(booking, cancellationToken);

            // Best-effort: the booking is ticketed and paid for whatever happens to
            // the email, so a PDF or mail failure is logged, never surfaced.
            if (ticket.StatusId == StatusTicketed)
            {
                try
                {
                    await _eTicketService.SendAsync(booking, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "E-ticket email failed for trip booking {TripBookingId}.", booking.Id);
                }
            }
        }
    }
}
