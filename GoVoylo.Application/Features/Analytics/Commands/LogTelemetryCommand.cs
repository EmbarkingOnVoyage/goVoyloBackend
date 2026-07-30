using MediatR;

namespace GoVoylo.Application.Features.Analytics.Commands;

public record LogTelemetryCommand(
    string AgentId,
    string EventName,
    DateTime Timestamp,
    Dictionary<string, string> Payload) : IRequest<Unit>;
