using System.Globalization;
using System.Text.Json;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.ProxyCheck
{
    // proxycheck.io v2: location, connection type (Residential / Wireless /
    // Business / Hosting / VPN / TOR), VPN and proxy flags and a 0-100 risk
    // score. Shared mobile-carrier IPs come back as Wireless, not as proxies.
    // The key is in the query string, so this client's request logging is off
    // (see Program.cs) and the URL never goes into a log message.
    public class ProxyCheckClient : IIpReputationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ProxyCheckOptions _options;
        private readonly ILogger<ProxyCheckClient> _logger;

        public ProxyCheckClient(HttpClient httpClient, IOptions<ProxyCheckOptions> options, ILogger<ProxyCheckClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => _options.Enabled;

        public async Task<IpReputation?> LookupAsync(string ipAddress, CancellationToken cancellationToken)
        {
            if (!IsConfigured) return null;

            var path = $"v2/{Uri.EscapeDataString(ipAddress)}?vpn=3&asn=1&risk=1";
            if (!string.IsNullOrWhiteSpace(_options.ApiKey)) path += $"&key={Uri.EscapeDataString(_options.ApiKey)}";

            try
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("proxycheck.io lookup failed with HTTP {StatusCode}.", (int)response.StatusCode);
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                return Parse(document.RootElement, ipAddress, _options.UnsafeRiskScore, _logger);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning("proxycheck.io lookup failed: {Error}", ex.GetType().Name);
                return null;
            }
        }

        // { "status": "ok" | "warning" | "denied" | "error", "message": ..., "<ip>": { ... } }
        public static IpReputation? Parse(JsonElement root, string ipAddress, int unsafeRiskScore, ILogger? logger = null)
        {
            var status = Text(root, "status");
            if (status != "ok" && status != "warning")
            {
                logger?.LogWarning("proxycheck.io lookup {Status}: {Message}", status, Text(root, "message"));
                return null;
            }

            // The answer is keyed by the address as proxycheck.io wrote it, which
            // can differ from ours in IPv6 formatting — take the one object entry.
            JsonElement ip = default;
            var found = root.TryGetProperty(ipAddress, out ip);
            if (!found)
            {
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object) { ip = property.Value; found = true; break; }
                }
            }
            if (!found) return null;

            var type = Text(ip, "type");
            var isTor = string.Equals(type, "TOR", StringComparison.OrdinalIgnoreCase);
            var isVpn = Text(ip, "vpn") == "yes" || string.Equals(type, "VPN", StringComparison.OrdinalIgnoreCase);
            var isProxy = Text(ip, "proxy") == "yes";
            var isHosting = string.Equals(type, "Hosting", StringComparison.OrdinalIgnoreCase);
            var risk = ip.TryGetProperty("risk", out var r) && r.TryGetInt32(out var riskValue) ? riskValue : 0;
            var isSecure = !isVpn && !isProxy && !isTor && risk < unsafeRiskScore;

            return new IpReputation(
                Text(ip, "city"), Text(ip, "region"), Text(ip, "isocode"), Text(ip, "provider"),
                Number(ip, "latitude"), Number(ip, "longitude"),
                type, isVpn, isProxy, isTor, isHosting, risk, isSecure);
        }

        private static string? Text(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()
                : null;

        private static decimal? Number(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var value)) return null;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)) return Math.Round(number, 6);
            if (value.ValueKind == JsonValueKind.String
                && decimal.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return Math.Round(parsed, 6);
            return null;
        }
    }
}
