using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Application.Features.Flights.Queries.GetCancellationQuote
{
    public class GetCancellationQuoteQueryHandler
        : IRequestHandler<GetCancellationQuoteQuery, CancellationQuoteDto>
    {
        // Same statuses as CancelTripBookingCommandHandler: only a ticketed booking
        // (not a hold, a failed booking or one still ticketing) has a refund to quote.
        private const string StatusBlocked = "33";
        private const string StatusFailed = "22";
        private const string StatusTicketingPending = "44";

        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly ILogger<GetCancellationQuoteQueryHandler> _logger;

        public GetCancellationQuoteQueryHandler(
            ITripBookingRepository tripBookingRepository,
            IFlightSupplierClientResolver supplierClientResolver,
            ILogger<GetCancellationQuoteQueryHandler> logger)
        {
            _tripBookingRepository = tripBookingRepository;
            _supplierClientResolver = supplierClientResolver;
            _logger = logger;
        }

        public async Task<CancellationQuoteDto> Handle(
            GetCancellationQuoteQuery request, CancellationToken cancellationToken)
        {
            var booking = await _tripBookingRepository.GetByIdAsync(request.TripBookingId, cancellationToken)
                ?? throw new NotFoundException("Booking not found.");

            if (booking.UserId != request.UserId)
            {
                throw new ForbiddenException("not_your_booking", "This booking does not belong to you.");
            }

            if (booking.LocalStatus != TripBooking.StatusActive)
            {
                throw new BusinessRuleException(
                    "already_cancelled", $"This booking has already been {booking.LocalStatus.ToLowerInvariant()}.");
            }

            if (booking.StatusId is StatusFailed or StatusBlocked or StatusTicketingPending)
            {
                throw new BusinessRuleException(
                    "not_ticketed", "Only a ticketed booking has a cancellation refund to quote.");
            }

            if (string.IsNullOrWhiteSpace(booking.AirlinePnr))
            {
                throw new BusinessRuleException(
                    "missing_pnr", "This booking has no airline PNR on file and can't be cancelled automatically.");
            }

            var airlinePnr = booking.AirlinePnr;
            string? origin = null, destination = null;
            DateTime? departureDate = null;
            if (request.LegIndex.HasValue)
            {
                var leg = booking.Legs.FirstOrDefault(l => l.LegIndex == request.LegIndex.Value)
                    ?? throw new BusinessRuleException(
                        "leg_not_found", $"Leg {request.LegIndex.Value} does not exist on this booking.");
                if (leg.IsCancelled)
                {
                    throw new BusinessRuleException("leg_already_cancelled", "This leg has already been cancelled.");
                }

                airlinePnr = leg.AirlinePnr ?? booking.AirlinePnr;
                origin = leg.Origin;
                destination = leg.Destination;
                departureDate = leg.TravelDate;
            }

            var supplierClient = _supplierClientResolver.Resolve(booking.SupplierCode);
            SupplierCancellationQuoteDto quote;
            try
            {
                quote = await supplierClient.GetCancellationQuoteAsync(
                    new SupplierCancellationQuoteRequestDto(
                        booking.BookingRefNo, airlinePnr, origin, destination, departureDate),
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException and not AppException)
            {
                // e.g. Tripjack 2563 "AutoCancellation is not enabled for supplier".
                _logger.LogWarning(ex, "Cancellation quote failed for trip booking {TripBookingId}.", booking.Id);
                throw new BusinessRuleException(
                    "quote_unavailable", "The airline can't quote a refund for this booking online right now.");
            }

            // The base/taxes split is only for the whole booking, and only nice to have.
            decimal? baseFare = null, taxes = null;
            if (!request.LegIndex.HasValue)
            {
                try
                {
                    var details = await supplierClient.GetBookingDetailsAsync(
                        booking.BookingRefNo, booking.AirlinePnr, cancellationToken);
                    baseFare = details.BaseFare;
                    taxes = details.TaxesAndFees;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex,
                        "Supplier booking details failed while quoting trip booking {TripBookingId}.", booking.Id);
                }
            }

            // A whole-booking cancel shows what the customer actually paid (including
            // add-ons); a leg-only one shows that leg's fare as the supplier reports it.
            var amountPaid = request.LegIndex.HasValue || booking.TotalAmount <= 0m
                ? quote.TotalFare
                : booking.TotalAmount;

            return new CancellationQuoteDto(
                amountPaid,
                quote.CancellationCharges,
                quote.RefundAmount,
                baseFare,
                taxes,
                quote.IsEstimate,
                Variant(quote, taxes),
                booking.CurrencyCode);
        }

        private static string Variant(SupplierCancellationQuoteDto quote, decimal? taxes)
        {
            if (quote.IsEstimate)
            {
                return CancellationQuoteVariants.Estimated;
            }

            if (quote.CancellationCharges <= 0m && quote.RefundAmount > 0m)
            {
                return CancellationQuoteVariants.FreeCancellation;
            }

            // Nothing back, or no more than the taxes (within a rupee), means the base
            // fare is forfeited.
            if (quote.RefundAmount <= 0m || (taxes.HasValue && quote.RefundAmount <= taxes.Value + 1m))
            {
                return CancellationQuoteVariants.NonRefundable;
            }

            return CancellationQuoteVariants.PartialRefund;
        }
    }
}
