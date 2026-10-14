namespace GoVoylo.Infrastructure.ExternalServices.ProxyCheck
{
    public class ProxyCheckOptions
    {
        // Off: rows are kept with lookup_status not_configured.
        public bool Enabled { get; set; } = true;

        // Optional: without a key proxycheck.io answers 100 lookups a day, with a
        // free account's key 1,000. Set as ProxyCheck__ApiKey in the App Service.
        public string ApiKey { get; set; } = string.Empty;

        // At or above this risk score an IP is treated as not secure
        // (proxycheck.io: 67-100 is very high risk).
        public int UnsafeRiskScore { get; set; } = 67;
    }
}
