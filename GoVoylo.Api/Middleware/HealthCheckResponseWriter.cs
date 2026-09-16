using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GoVoylo.Api.Middleware
{
    public static class HealthCheckResponseWriter
    {
        public static Task WriteJsonAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
