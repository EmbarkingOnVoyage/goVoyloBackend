using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;

namespace GoVoylo.Infrastructure.Persistence.Repositories;

public class InMemoryActivityLogRepository : IActivityLogRepository
{
    // A simple standard C# list acting as your temporary data dump
    private readonly List<UserActivityLog> _logs = new();

    public Task LogActivityAsync(UserActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        // Simply add to the in-memory array list so unit tests remain happy and fast
        _logs.Add(activityLog);
        return Task.CompletedTask;
    }
}
