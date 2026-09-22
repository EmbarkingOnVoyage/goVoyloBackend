using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{
    public static class RazorpayConstants
    {
        public const string ProviderName = "Razorpay";

        public const string OrdersEndpoint =
            "https://api.razorpay.com/v1/orders";

        public const string PaymentsEndpoint =
            "https://api.razorpay.com/v1/payments";
    }
}
