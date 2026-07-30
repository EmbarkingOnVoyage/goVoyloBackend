// GoVoylo.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using GoVoylo.Infrastructure.Persistence.Repositories;

namespace GoVoylo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 1. Setup an InMemory database context for local development/testing 
        // This acts as temporary PostgreSQL database without needing Docker or a live instance setup yet
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("GoVoyloDb"));

        // 2. Register Repository interfaces and concrete classes (Scoped lifestyle)
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        return services;
    }
}
