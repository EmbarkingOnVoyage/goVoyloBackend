using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Dtos
{
    public record PaymentOrderResult(
    string OrderId,
    decimal Amount,
    string Currency
);
}
