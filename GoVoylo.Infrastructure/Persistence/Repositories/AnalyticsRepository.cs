using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InsertLogAsync(AnalyticsTelemetry telemetry)
        {

        }

    }

}
