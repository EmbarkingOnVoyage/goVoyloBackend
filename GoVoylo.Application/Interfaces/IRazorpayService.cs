using GoVoylo.Application.Features.Payments.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IRazorpayService
    {
        Task<RazorpayOrderResponse> CreateOrderAsync(
        decimal amount,
        string currency,
        string receipt,
        CancellationToken cancellationToken);

        bool VerifyPaymentSignature(
            string orderId,
            string paymentId,
            string signature);
    }
}
