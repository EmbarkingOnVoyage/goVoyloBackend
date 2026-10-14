using System.Linq;
using MediatR;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Application.Interfaces;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public class VerifyRazorpayPaymentCommandHandler : IRequestHandler<VerifyRazorpayPaymentCommand, PaymentResponseDto>
{
    // Flyshop's own Status_Id convention (Air_Ticketing docs): 33-Block is the only
    // state AddPayment + Book_Ticket should ever run against here — a booking that's
    // already ticketed or already failed has nothing left to do.
    private const string StatusBlocked = "33";

    private readonly IPaymentRepository _paymentRepository;
    private readonly IRazorpayClient _razorpayClient;
    private readonly ITripBookingRepository _tripBookingRepository;
    private readonly ITripBookingTicketingService _ticketingService;
    private readonly IAccessAuditService _accessAuditService;
    private readonly ILogger<VerifyRazorpayPaymentCommandHandler> _logger;

    public VerifyRazorpayPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IRazorpayClient razorpayClient,
        ITripBookingRepository tripBookingRepository,
        ITripBookingTicketingService ticketingService,
        IAccessAuditService accessAuditService,
        ILogger<VerifyRazorpayPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _razorpayClient = razorpayClient;
        _tripBookingRepository = tripBookingRepository;
        _ticketingService = ticketingService;
        _accessAuditService = accessAuditService;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> Handle(VerifyRazorpayPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByGatewayOrderIdAsync(request.RazorpayOrderId, cancellationToken)
            ?? throw new NotFoundException($"No payment found for Razorpay order '{request.RazorpayOrderId}'.");

        var isValid = _razorpayClient.VerifySignature(
            request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

        if (!isValid)
        {
            payment.MarkAsFailed();
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            _accessAuditService.LogPaymentCompleted(request.RazorpayOrderId, null, "Failed");
            throw new BusinessRuleException("invalid_payment_signature", "Payment signature verification failed.");
        }

        payment.MarkAsSucceeded(request.RazorpayPaymentId);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        _accessAuditService.LogPaymentCompleted(request.RazorpayOrderId, request.RazorpayPaymentId, "Succeeded");

        // The customer's Razorpay payment has genuinely succeeded by this point.
        // payment.BookingReference is the same Flyshop Booking_RefNo the hold was
        // created under (TravelerDetailsScreen.handlePayNow passes the real
        // CreateBooking result's bookingRefNo, not a synthetic client-side id), so
        // this lookup is never ambiguous between BookingPayment and TripBooking.
        var booking = await _tripBookingRepository.GetByBookingRefNoAsync(payment.BookingReference, cancellationToken);

        // Saved before ticketing, so a booking that then fails to ticket still
        // shows what the customer paid.
        if (booking != null)
        {
            booking.RecordPayment(payment.TotalAmount);
            await _tripBookingRepository.UpdateAsync(booking, cancellationToken);
        }

        // A hold released before this payment arrived (an abandoned checkout the
        // expiry job gave up on) has nothing left to ticket: the payment stays
        // recorded on it for a manual refund.
        if (booking != null && booking.StatusId == StatusBlocked && booking.LocalStatus != TripBooking.StatusActive)
        {
            _logger.LogError(
                "Payment {PaymentId} verified for released booking {BookingRefNo}; not ticketing — refund needed.",
                payment.Id, booking.BookingRefNo);
        }
        else if (booking != null && booking.StatusId == StatusBlocked)
        {
            await _ticketingService.TicketAsync(booking, payment.Id.ToString(), cancellationToken);
        }

        return new PaymentResponseDto(
            payment.Id,
            payment.BookingReference,
            payment.TotalAmount,
            payment.Currency,
            payment.PaymentStatus,
            payment.CreatedAt);
    }
}
