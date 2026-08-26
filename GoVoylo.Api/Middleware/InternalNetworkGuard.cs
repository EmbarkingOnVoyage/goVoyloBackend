using System.Net;

namespace GoVoylo.Api.Middleware
{
    // Application-level stopgap for "restrict to internal only" — the real fix is
    // network-level (load balancer / firewall rule), which isn't provisioned yet
    // since hosting infrastructure doesn't exist for this project. Revisit once it
    // does: behind a reverse proxy, Kestrel sees the proxy's IP unless forwarded
    // headers are configured to match that specific topology.
    public static class InternalNetworkGuard
    {
        public static bool IsInternal(IPAddress? address)
        {
            if (address == null)
            {
                return false;
            }

            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }

            var bytes = address.GetAddressBytes();

            if (bytes.Length == 4)
            {
                // 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16
                return bytes[0] == 10
                    || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
                    || (bytes[0] == 192 && bytes[1] == 168);
            }

            // fc00::/7 — IPv6 unique local addresses
            return bytes.Length == 16 && (bytes[0] & 0xfe) == 0xfc;
        }
    }
}
