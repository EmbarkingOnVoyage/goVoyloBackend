using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Dtos
{
 public record CreatePaymentOrderResponseDto(
        Guid PaymentId,
        string BookingReference,
        decimal SupplierAmount,
        decimal CommissionAmount,
        decimal CustomerPayableAmount,
        string Currency,
        string PaymentProvider,
        string ProviderOrderId,
        string? PublicKey,
        string? CheckoutToken
);

}
