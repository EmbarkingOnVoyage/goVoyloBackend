using GoVoylo.Infrastructure.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.Monitoring
{
    public class EmailServiceHealthCheck : IHealthCheck
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailServiceHealthCheck(IOptions<SmtpSettings> smtpOptions)
        {
            _smtpSettings = smtpOptions.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
            {
                return HealthCheckResult.Unhealthy("SMTP is not configured.");
            }

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                return HealthCheckResult.Healthy("SMTP server reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SMTP server unreachable.", ex);
            }
        }
    }
}
