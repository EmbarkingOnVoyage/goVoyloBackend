using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Infrastructure.Jobs
{
    public class AirportImportService
    {
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

            foreach (var (iata, name, city, country, popular) in AirportSeedData.Airports)
            {
                await _airportRepository.AddAsync(new Airport(iata, name, city, country, popular));
            }

            _logger.LogInformation("Imported {Count} airports from starter seed data.", AirportSeedData.Airports.Count);
        }
    }
}
