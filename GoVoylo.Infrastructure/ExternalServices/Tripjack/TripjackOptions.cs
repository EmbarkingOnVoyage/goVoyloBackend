namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    // Tripjack authenticates every call (both pre-booking FMS and post-booking OMS
    // endpoints) with a single "apikey" HTTP header — confirmed against their real
    // Flights API v2.0 docs (tripjack.com/page/api-doc) and their own "API Key
    // Configuration" partner guide. There is no body-embedded auth object like
    // Flyshop's Auth_Header; UAT and production use separate keys, and the key is
    // itself IP-whitelisted on Tripjack's side (see the same partner guide) — calls
    // from a non-whitelisted IP fail with "Access Denied" regardless of the key.
    public class TripjackOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        // UAT certification log capture — see TripjackWireLogHandler.
        public bool CaptureWireLogs { get; set; }
        public string WireLogDirectory { get; set; } = string.Empty;
    }
}
