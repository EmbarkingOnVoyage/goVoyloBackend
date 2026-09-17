using GoVoylo.Application.Features.Payments.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IRazorpayService
    {
        Task<PaymentOrderResult> CreateOrderAsync(
        decimal amount,
        string currency,
        string bookingReference,
        CancellationToken cancellationToken);

        Task<bool> VerifyPaymentAsync(
            string orderId,
            string paymentId,
            string signature,
            CancellationToken cancellationToken);
    }
}
