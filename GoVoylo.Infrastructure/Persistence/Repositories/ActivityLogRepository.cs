// GoVoylo.Infrastructure/Persistence/Repositories/ActivityLogRepository.cs
using System.Diagnostics;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;

namespace GoVoylo.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    public Task LogActivityAsync(UserActivityLog log)
    {
        // For now, we simulate logging to MongoDB console out for local development.
        // In a production environment, this is where MongoCollection.InsertOneAsync() lives.
        Debug.WriteLine($"[MongoDB Dump] Action: {log.ActionType} from {log.SourcePlatform}");
        
        return Task.CompletedTask;
    }
}
