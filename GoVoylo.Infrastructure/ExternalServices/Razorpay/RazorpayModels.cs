using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{
    public record RazorpayCreateOrderRequest(
    int amount,
    string currency,
    string receipt,
    //object[]? transfers
        RazorpayTransfer[]? transfers

);

    public record RazorpayCreateOrderResponse(
        string id,
        int amount,
        string currency
    );

    public record RazorpayTransfer(
        string account,
        int amount,
        string currency,
        object notes
    );
}
