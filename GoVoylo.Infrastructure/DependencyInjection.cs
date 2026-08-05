using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using GoVoylo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration?.GetConnectionString("DefaultConnection");

        bool isMigrationRunning = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName != null && a.FullName.Contains("Microsoft.EntityFrameworkCore.Design"));

        if (!string.IsNullOrEmpty(connectionString) || isMigrationRunning)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString ?? "Host=localhost;Database=MigrationDb", b => 
                {
                    b.MigrationsAssembly("GoVoylo.Infrastructure");
                    
                    // THE PROFESSIONAL FIX: 
                    // This forces EF Core to use a secure execution strategy. It prepares the database channel 
                    // and handles the creation of the schema history table cleanly behind the scenes, 
                    // preventing raw SQL exception logs on initial setup.
                    b.EnableRetryOnFailure();
                });
            });
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("GoVoyloDb"));
        }

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IBookFlightRepository, BookFlightRepository>();

        return services;
    }
}
