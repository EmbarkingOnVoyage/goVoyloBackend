using System.Diagnostics;
using GoVoylo.Infrastructure.ExternalServices.Tripjack;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.Monitoring
{
    public class TripjackHealthCheck : IHealthCheck
    {
        private readonly TripjackOptions _options;

        public TripjackHealthCheck(IOptions<TripjackOptions> options)
        {
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                return HealthCheckResult.Unhealthy("Tripjack is not configured (no BaseUrl).");
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                using var response = await client.GetAsync(_options.BaseUrl, cancellationToken);
                stopwatch.Stop();

                var data = new Dictionary<string, object> { ["responseTimeMs"] = stopwatch.ElapsedMilliseconds };

                // Any response at all — even a 404/405 for GET on a POST-only endpoint —
                // proves the host is reachable, which is what this check is for.
                return HealthCheckResult.Healthy(
                    $"Tripjack host reachable in {stopwatch.ElapsedMilliseconds}ms.", data);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return HealthCheckResult.Unhealthy("Tripjack host unreachable.", ex);
            }
        }
    }
}
