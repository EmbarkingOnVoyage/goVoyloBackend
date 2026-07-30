using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task InsertLogAsync(AnalyticsTelemetry telemetry);
    }
}