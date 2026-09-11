using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Dtos
{
    public record CreatePaymentOrderResponseDto(
    Guid PaymentId,
    string BookingReference,
    decimal Amount,
    string Currency,
    string RazorpayKeyId,
    string RazorpayOrderId
);
}
