using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GoVoylo.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public LoggingBehavior(ILogger<TRequest> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Executing Application Command/Query: {Name}", requestName);
        
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        _logger.LogInformation("Completed {Name} in {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);
        return response;
    }
}
