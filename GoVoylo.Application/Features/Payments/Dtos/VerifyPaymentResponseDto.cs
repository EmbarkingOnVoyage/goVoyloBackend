using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Dtos
{
    public record VerifyPaymentResponseDto(
    Guid PaymentId,
    string PaymentStatus,
    string PaymentProvider,
    string ProviderOrderId,
    string? ProviderPaymentId,
    DateTime? PaidAt
);
}
