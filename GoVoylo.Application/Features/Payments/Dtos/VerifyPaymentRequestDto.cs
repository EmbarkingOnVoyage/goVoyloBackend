using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Dtos
{
    public record VerifyPaymentRequestDto(
    Guid PaymentId,
    string PaymentProvider,
    string ProviderOrderId,
    string ProviderPaymentId,
    string ProviderSignature
);
}
