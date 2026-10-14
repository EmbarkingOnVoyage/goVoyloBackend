namespace GoVoylo.Application.Interfaces
{
    public record IpReputation(
        string? City,
        string? Region,
        string? CountryCode,
        string? Isp,
        decimal? Latitude,
        decimal? Longitude,
        string? ConnectionType,
        bool IsVpn,
        bool IsProxy,
        bool IsTor,
        bool IsHosting,
        int RiskScore,
        bool IsSecure);

    // Location and safety of an IP address (proxycheck.io).
    public interface IIpReputationClient
    {
        // False when lookups are switched off — rows are then kept without them.
        bool IsConfigured { get; }

        // Null when the service didn't answer usefully.
        Task<IpReputation?> LookupAsync(string ipAddress, CancellationToken cancellationToken);
    }
}
