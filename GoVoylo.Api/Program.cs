using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure;
using GoVoylo.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
namespace GoVoylo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- 1. EXISTING TEMPLATE SERVICES ---
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddControllers(); // Add this line so .NET finds your PaymentsController

        // --- 2. CLEAN ARCHITECTURE SERVICE WIRE UP ---
        // Registers your repository interfaces and your In-Memory database
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        builder.Services.AddScoped<IBookFlightRepository, BookFlightRepository>();

        // Registers MediatR and scans your Application project for Handlers
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GoVoylo.Application.Features.Payments.Commands.ProcessPayment.ProcessPaymentCommand).Assembly));

        // --- 3. HEAVY TRAFFIC RATE LIMITING DEFENSE ---
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("HeavyTrafficPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });

        var app = builder.Build();

        // --- 4. HTTP PIPELINE MIDDLEWARE CONFIGURATION ---
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // Enable the heavy traffic protection middleware
        app.UseRateLimiter();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Map your controllers so the API routing endpoints actually work
        app.MapControllers();

        app.Run();
    }

}
