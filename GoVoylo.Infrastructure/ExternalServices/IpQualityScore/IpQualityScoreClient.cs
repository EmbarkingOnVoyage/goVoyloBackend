using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.IpQualityScore
{
    // IPQualityScore's IP reputation API: location plus VPN / proxy / Tor / abuse
    // flags and a 0-100 fraud score. The key is part of the URL path, so this
    // client's HTTP request logging is switched off (see Program.cs) and the URL
    // never goes into an exception or log message.
    public class IpQualityScoreClient : IIpReputationClient
    {
        private readonly HttpClient _httpClient;
        private readonly IpQualityScoreOptions _options;
        private readonly ILogger<IpQualityScoreClient> _logger;

        public IpQualityScoreClient(HttpClient httpClient, IOptions<IpQualityScoreOptions> options, ILogger<IpQualityScoreClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

        public async Task<IpReputation?> LookupAsync(string ipAddress, CancellationToken cancellationToken)
        {
            if (!IsConfigured) return null;

            var path = $"api/json/ip/{Uri.EscapeDataString(_options.ApiKey)}/{Uri.EscapeDataString(ipAddress)}"
                + "?strictness=1&allow_public_access_points=true&lighter_penalties=true";
            try
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("IPQualityScore lookup failed with HTTP {StatusCode}.", (int)response.StatusCode);
                    return null;
                }

                var wire = await response.Content.ReadFromJsonAsync<IpQualityScoreResponseWire>(cancellationToken: cancellationToken);
                if (wire == null || !wire.Success)
                {
                    _logger.LogWarning("IPQualityScore lookup unsuccessful: {Message}", wire?.Message);
                    return null;
                }

                var isSecure = !wire.Vpn && !wire.Proxy && !wire.Tor && !wire.RecentAbuse && !wire.BotStatus
                    && wire.FraudScore < _options.UnsafeFraudScore;

                return new IpReputation(
                    Blank(wire.City), Blank(wire.Region), Blank(wire.CountryCode), Blank(wire.Isp),
                    wire.Latitude, wire.Longitude,
                    wire.Vpn, wire.Proxy, wire.Tor, wire.RecentAbuse, wire.BotStatus, wire.FraudScore, isSecure);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning("IPQualityScore lookup failed: {Error}", ex.GetType().Name);
                return null;
            }
        }

        // IPQualityScore answers "N/A" for fields it doesn't know.
        private static string? Blank(string? value) =>
            string.IsNullOrWhiteSpace(value) || value == "N/A" ? null : value;
    }

    public class IpQualityScoreResponseWire
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("fraud_score")] public int FraudScore { get; set; }
        [JsonPropertyName("country_code")] public string? CountryCode { get; set; }
        [JsonPropertyName("region")] public string? Region { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("ISP")] public string? Isp { get; set; }
        [JsonPropertyName("latitude")] public decimal? Latitude { get; set; }
        [JsonPropertyName("longitude")] public decimal? Longitude { get; set; }
        [JsonPropertyName("proxy")] public bool Proxy { get; set; }
        [JsonPropertyName("vpn")] public bool Vpn { get; set; }
        [JsonPropertyName("tor")] public bool Tor { get; set; }
        [JsonPropertyName("recent_abuse")] public bool RecentAbuse { get; set; }
        [JsonPropertyName("bot_status")] public bool BotStatus { get; set; }
    }
}
