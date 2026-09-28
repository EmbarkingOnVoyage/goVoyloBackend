using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CancelTripBooking
{
    public class CancelTripBookingCommandHandler
        : IRequestHandler<CancelTripBookingCommand, CancelTripBookingResponseDto>
    {
        // Air_Ticketing's own docs: 11-Success (ticketed), 22-Failed, 33-Block (hold).
        // A hold has to be released via Air_ReleasePNR — confirmed live against UAT
        // that Air_TicketCancellation rejects an un-ticketed hold outright
        // (Err007 "Please check your RefNo Status") — while an actual ticket is
        // cancelled via Air_TicketCancellation instead. See FLIGHT_ANCILLARIES_SCOPE.MD.
        private const string StatusBlocked = "33";
        private const string StatusFailed = "22";

        // "015" = "Please cancel my ticket with Applicable Penalty" — the generic
        // customer-initiated reason from Air_TicketCancellation's own CancelCode table,
        // used for every My Trips cancellation rather than asking the user to pick one.
        private const string CustomerCancelCode = "015";

        // Every segment cancelled here is assumed non-connecting (Segment_Id 0) — the
        // same simplification already verified live for every booking this app has
        // created. A genuinely connecting (multi-segment) flight would need per-segment
        // ids that aren't captured at booking time today.
        private const string DirectFlightSegmentId = "0";

        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly IFlightSupplierClientResolver _supplierClientResolver;

        public CancelTripBookingCommandHandler(
            ITripBookingRepository tripBookingRepository,
            IFlightSupplierClientResolver supplierClientResolver)
        {
            _tripBookingRepository = tripBookingRepository;
            _supplierClientResolver = supplierClientResolver;
        }

        public async Task<CancelTripBookingResponseDto> Handle(
            CancelTripBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _tripBookingRepository.GetByIdAsync(request.TripBookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundException("Booking not found.");
            }

            if (booking.UserId != request.UserId)
            {
                throw new ForbiddenException("not_your_booking", "This booking does not belong to you.");
            }

            if (booking.LocalStatus != TripBooking.StatusActive)
            {
                throw new BusinessRuleException(
                    "already_cancelled", $"This booking has already been {booking.LocalStatus.ToLowerInvariant()}.");
            }

            if (booking.StatusId == StatusFailed)
            {
                throw new BusinessRuleException(
                    "nothing_to_cancel", "This booking was never held or ticketed with the airline.");
            }

            if (string.IsNullOrWhiteSpace(booking.AirlinePnr))
            {
                throw new BusinessRuleException(
                    "missing_pnr", "This booking has no airline PNR on file and can't be cancelled automatically.");
            }

            if (request.LegIndex.HasValue && booking.StatusId == StatusBlocked)
            {
                throw new BusinessRuleException(
                    "leg_cancel_not_supported_for_hold",
                    "A held (un-ticketed) booking must be released as a whole, not by leg.");
            }

            var supplierClient = _supplierClientResolver.Resolve(booking.SupplierCode);

            if (booking.StatusId == StatusBlocked)
            {
                await supplierClient.ReleaseHoldAsync(
                    new SupplierReleaseHoldRequestDto(booking.BookingRefNo, booking.AirlinePnr),
                    cancellationToken);

                booking.MarkReleased();
            }
            else
            {
                var cancellationType = request.CancellationType ?? 0;
                var cancelCode = request.CancelCode ?? CustomerCancelCode;

                var paxIds = booking.PaxIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var legsToCancel = booking.Legs.AsEnumerable();
                // Defaults to the booking-level PNR (correct for a oneway, and for a
                // roundtrip whose legs share one PNR). Overridden below for a
                // leg-specific cancel, since a roundtrip's two legs can legitimately
                // have been ticketed under different Airline_PNRs (see
                // VerifyRazorpayPaymentCommandHandler) — sending the wrong leg's PNR
                // would cancel (or fail to cancel) the wrong reservation at Flyshop.
                var airlinePnr = booking.AirlinePnr;
                if (request.LegIndex.HasValue)
                {
                    var targetLeg = booking.Legs.FirstOrDefault(l => l.LegIndex == request.LegIndex.Value);
                    if (targetLeg == null)
                    {
                        throw new BusinessRuleException(
                            "leg_not_found", $"Leg {request.LegIndex.Value} does not exist on this booking.");
                    }
                    if (targetLeg.IsCancelled)
                    {
                        throw new BusinessRuleException(
                            "leg_already_cancelled", "This leg has already been cancelled.");
                    }
                    legsToCancel = new[] { targetLeg };
                    airlinePnr = targetLeg.AirlinePnr ?? booking.AirlinePnr;
                }

                var segments = legsToCancel
                    .SelectMany(leg => paxIds
                        .Select(paxId => new SupplierCancelSegmentDto(leg.FlightId, paxId, DirectFlightSegmentId)))
                    .ToList();

                await supplierClient.CancelBookingAsync(
                    new SupplierCancellationRequestDto(
                        booking.BookingRefNo,
                        airlinePnr,
                        cancellationType,
                        cancelCode,
                        "Cancelled by customer via GoVoylo app",
                        segments),
                    cancellationToken);

                if (request.LegIndex.HasValue)
                {
                    booking.MarkLegCancelled(request.LegIndex.Value, cancellationType, cancelCode);
                }
                else
                {
                    booking.MarkCancelled(cancellationType, cancelCode);
                }
            }

            await _tripBookingRepository.UpdateAsync(booking, cancellationToken);

            return new CancelTripBookingResponseDto(true, booking.LocalStatus);
        }
    }
}
