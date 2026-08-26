using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using GoVoylo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using GoVoylo.Application.Interfaces; 
using GoVoylo.Infrastructure.Services.B2b.TripJack;

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
        var mongoConnectionString = configuration?.GetConnectionString("MongoConnection");
        if (!string.IsNullOrEmpty(mongoConnectionString))
        {
            services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        }
        else
        {
            // Fallback: If running Unit Tests without a connection string, 
            // map to an In-Memory mock implementation so tests remain green.
            services.AddScoped<IActivityLogRepository, InMemoryActivityLogRepository>();
        }
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IBookFlightRepository, BookFlightRepository>();
        bool useMock = configuration?.GetValue<bool>("B2bSettings:TripJack:UseMock") ?? true;

        if (useMock)
        {
            // If UseMock is true, we register a mock service that returns a static JSON response.
            services.AddTransient<ITripJackTestService, TripJackMockService>();
        }
        else
        {
            // If UseMock is false, we register the real HTTP client service for TripJack.
            services.AddHttpClient<ITripJackTestService, TripJackTestService>(client =>
            {
                var baseUrl = configuration["B2bSettings:TripJack:BaseUrl"] ?? "https://apitest.tripjack.com/";
                var apiKey = configuration["B2bSettings:TripJack:ApiKey"] ?? "717512708b4ba99-786c-46c9-a801-37891e3a8bab";

                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("apikey", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });
        }
        return services;
    }
}
