using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.ExternalServices.Cashfree
{
    public static class CashfreeConstants
    {
        public const string ProviderName = "Cashfree";

        public const string SandboxBaseUrl =
            "https://sandbox.cashfree.com/pg";

        public const string ProductionBaseUrl =
            "https://api.cashfree.com/pg";
    }
}
