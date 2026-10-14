using System.Net;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Api.Services
{
    public class ClientRequestInfo : IClientRequestInfo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientRequestInfo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // App Service's front end appends the caller's address (often "ip:port")
        // to X-Forwarded-For, so the last entry is the one it saw — earlier
        // entries can be anything the client chose to send. Without the header
        // (local runs) the connection's own address is used.
        public string? IpAddress
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrWhiteSpace(forwarded))
                {
                    var last = forwarded.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    var parsed = last == null ? null : ParseAddress(last);
                    if (parsed != null) return parsed;
                }

                var remote = context.Connection.RemoteIpAddress;
                if (remote == null) return null;
                return (remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote).ToString();
            }
        }

        public string? UserAgent
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        // "1.2.3.4", "1.2.3.4:5678", "2001:db8::1" or "[2001:db8::1]:5678".
        public static string? ParseAddress(string value)
        {
            if (IPAddress.TryParse(value, out var address))
                return (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();
            if (IPEndPoint.TryParse(value, out var endpoint))
                return (endpoint.Address.IsIPv4MappedToIPv6 ? endpoint.Address.MapToIPv4() : endpoint.Address).ToString();
            return null;
        }
    }
}
