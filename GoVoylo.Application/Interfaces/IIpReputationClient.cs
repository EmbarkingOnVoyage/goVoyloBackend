namespace GoVoylo.Application.Interfaces
{
    public record IpReputation(
        string? City,
        string? Region,
        string? CountryCode,
        string? Isp,
        decimal? Latitude,
        decimal? Longitude,
        bool IsVpn,
        bool IsProxy,
        bool IsTor,
        bool RecentAbuse,
        bool IsBot,
        int FraudScore,
        bool IsSecure);

    // Location and safety of an IP address (IPQualityScore).
    public interface IIpReputationClient
    {
        // False when no API key is set — lookups are then skipped.
        bool IsConfigured { get; }

        // Null when the service didn't answer usefully.
        Task<IpReputation?> LookupAsync(string ipAddress, CancellationToken cancellationToken);
    }
}
