using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;

namespace GoVoylo.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    // STEP 1: Declare the pipeline field.
    // IMongoCollection<T> is a built-in interface from the MongoDB.Driver NuGet package.
    // Think of this exactly like an EF Core 'DbSet<UserActivityLog>', but for NoSQL documents.
    private readonly IMongoCollection<UserActivityLog> _activityLogs;

    public ActivityLogRepository(IConfiguration configuration)
    {
        // STEP 2: Dial the Docker container server link.
        var connectionString = configuration.GetConnectionString("MongoConnection");
        var client = new MongoClient(connectionString);
        
        // STEP 3: Grab the specific database scope.
        var database = client.GetDatabase("govoylo_analytics_db");
        
        // STEP 4: Initialize our field here!
        // We tell the database engine to open a pipeline to a collection named "UserActivityLogs".
        // This is where '_activityLogs' officially comes into the picture.
        _activityLogs = database.GetCollection<UserActivityLog>("UserActivityLogs");
    }

    public async Task LogActivityAsync(UserActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        // STEP 5: Push the data down the active pipeline.
        // We use the initialized field to fire the insert command straight into Docker.
        await _activityLogs.InsertOneAsync(activityLog, cancellationToken: cancellationToken);
    }
}
