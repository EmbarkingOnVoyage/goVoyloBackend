using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Dtos
{
    public record PaymentOrderRequest(
        string BookingReference,
        decimal TotalAmount,
        decimal SupplierAmount,
        decimal CommissionAmount,
        string Currency
   );
   
}
