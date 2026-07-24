using MediatR;
using Microsoft.Extensions.Logging;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Analytics.Commands;

public class LogTelemetryCommandHandler : IRequestHandler<LogTelemetryCommand, Unit>
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<LogTelemetryCommandHandler> _logger;

    public LogTelemetryCommandHandler(
        IAnalyticsRepository analyticsRepository, 
        ILogger<LogTelemetryCommandHandler> logger)
    {
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogTelemetryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing telemetry log for Agent: {AgentId}, Event: {EventName}", 
            request.AgentId, request.EventName);

        // Instantiate using your exact domain constructor discovered via error code CS7036
        var telemetryRecord = new AnalyticsTelemetry(
            request.AgentId,
            request.EventName,
            request.Payload
        );

        // Persist via your exact interface method
        await _analyticsRepository.InsertLogAsync(telemetryRecord);

        return Unit.Value;
    }
}
