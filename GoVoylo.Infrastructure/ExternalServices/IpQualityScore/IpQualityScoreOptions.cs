namespace GoVoylo.Infrastructure.ExternalServices.IpQualityScore
{
    public class IpQualityScoreOptions
    {
        // Empty switches IP lookups off (rows are kept with lookup_status
        // not_configured). Set as IpQualityScore__ApiKey in the App Service.
        public string ApiKey { get; set; } = string.Empty;

        // At or above this fraud score an IP is treated as not secure.
        // IPQualityScore calls 75+ suspicious and 85+ high risk.
        public int UnsafeFraudScore { get; set; } = 75;
    }
}
