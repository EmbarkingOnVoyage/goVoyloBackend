using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{
    public class RazorpayOptions
    {
        public string KeyId { get; set; } = string.Empty;

        public string KeySecret { get; set; } = string.Empty;

        public string CommissionAccountId { get; set; } = string.Empty;

        public string SupplierAccountId { get; set; } = string.Empty;
        public bool EnableTransfers { get; internal set; }
    }
}
