using System.Text.Json;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Infrastructure.Jobs
{
    public class AirportImportService
    {
        private const string ResourceName = "GoVoylo.Infrastructure.SeedData.airports.json";

        // A short list of major hubs to flag popular in the imported master
        // data — mirrors the "Popular" set AirportSeedData.cs (the file this
        // replaces) used to hardcode, so the /popular quick-suggestion chips
        // keep behaving the same after the swap.
        private static readonly HashSet<string> PopularIataCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "BOM", "DEL", "BLR", "MAA", "HYD", "DXB", "SIN", "BKK", "LHR", "JFK",
        };

        private record AirportSeedRow(string Code, string City, string State, string Country, string Name);

        private readonly IAirportRepository _airportRepository;
        private readonly ILogger<AirportImportService> _logger;

        public AirportImportService(IAirportRepository airportRepository, ILogger<AirportImportService> logger)
        {
            _airportRepository = airportRepository;
            _logger = logger;
        }

        // Idempotent: only runs the import when the table is empty, so it's safe to call
        // on every startup without needing a separate "has this run before" flag.
        public async Task ImportIfEmptyAsync()
        {
            var existingCount = await _airportRepository.CountAsync();

            if (existingCount > 0)
            {
                return;
            }

            var rows = await LoadSeedRowsAsync();

            var airports = rows.Select(r => new Airport(
                r.Code, r.Name, r.City, r.Country, PopularIataCodes.Contains(r.Code)));

            await _airportRepository.AddRangeAsync(airports);

            _logger.LogInformation("Imported {Count} airports from the IATA master dataset.", rows.Count);
        }

        private static async Task<List<AirportSeedRow>> LoadSeedRowsAsync()
        {
            var assembly = typeof(AirportImportService).Assembly;
            await using var stream = assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

            var rows = await JsonSerializer.DeserializeAsync<List<AirportSeedRow>>(
                stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return rows ?? new List<AirportSeedRow>();
        }
    }
}
