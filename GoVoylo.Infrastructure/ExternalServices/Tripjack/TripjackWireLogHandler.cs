using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    // Tripjack's UAT certification requires the raw JSON request/response of every
    // call per test case, one file each, response unmodified (tripjack.com/page/api-doc,
    // "UAT Certification" → "Log Format Rules"). This handler sits under
    // TripjackClient's HttpClient and writes exactly the bytes sent and received, so
    // the logs are what went over the wire rather than a re-serialization of our own
    // wire models. Off unless TripjackSettings:CaptureWireLogs is true.
    //
    // Files land in {WireLogDirectory}/{yyyyMMdd}/{HHmmss-fff}_{seq}_{endpoint}_{Request|Response}.json
    // (UTC). On the Linux App Service the default is /home/LogFiles/tripjack-wire,
    // which persists across restarts and is included in `az webapp log download`.
    public class TripjackWireLogHandler : DelegatingHandler
    {
        private static int _sequence;

        private readonly TripjackOptions _options;
        private readonly ILogger<TripjackWireLogHandler> _logger;

        public TripjackWireLogHandler(
            IOptions<TripjackOptions> options, ILogger<TripjackWireLogHandler> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_options.CaptureWireLogs)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var now = DateTime.UtcNow;
            var sequence = Interlocked.Increment(ref _sequence);
            var endpoint = (request.RequestUri?.AbsolutePath ?? "unknown").Trim('/').Replace('/', '_');
            var filePrefix = Path.Combine(
                ResolveDirectory(), now.ToString("yyyyMMdd"), $"{now:HHmmss-fff}_{sequence:D4}_{endpoint}");

            if (request.Content != null)
            {
                // Buffer once and send the same bytes we log, so the logged request
                // is byte-for-byte what Tripjack received.
                var requestBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                request.Content = CopyWithHeaders(requestBytes, request.Content.Headers);
                await WriteSafelyAsync($"{filePrefix}_Request.json", requestBytes);
            }

            var response = await base.SendAsync(request, cancellationToken);

            var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            response.Content = CopyWithHeaders(responseBytes, response.Content.Headers);
            await WriteSafelyAsync($"{filePrefix}_Response.json", responseBytes);

            return response;
        }

        private string ResolveDirectory()
        {
            if (!string.IsNullOrWhiteSpace(_options.WireLogDirectory))
            {
                return _options.WireLogDirectory;
            }

            var home = Environment.GetEnvironmentVariable("HOME");
            return string.IsNullOrWhiteSpace(home)
                ? Path.Combine(AppContext.BaseDirectory, "tripjack-wire")
                : Path.Combine(home, "LogFiles", "tripjack-wire");
        }

        private static ByteArrayContent CopyWithHeaders(byte[] bytes, HttpContentHeaders headers)
        {
            var content = new ByteArrayContent(bytes);
            foreach (var header in headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return content;
        }

        // A logging failure must never break a real booking call.
        private async Task WriteSafelyAsync(string path, byte[] bytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllBytesAsync(path, bytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write Tripjack wire log {Path}", path);
            }
        }
    }
}
