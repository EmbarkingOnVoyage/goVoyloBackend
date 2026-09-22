using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.ExternalServices.Cashfree
{
    public class CashfreeOptions
    {
        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string Environment { get; set; } = "sandbox";

        public string ApiVersion { get; set; } = "2025-01-01";
    }
}
