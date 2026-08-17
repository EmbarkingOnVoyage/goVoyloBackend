using DotNetEnv;
using FluentValidation;
using GoVoylo.Application.Common.Behaviors;
using GoVoylo.Application.Features.Authentication.Commands.Register;
using GoVoylo.Application.Interfaces;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure;
using GoVoylo.Infrastructure.Persistence.Repositories;
using GoVoylo.Infrastructure.Services;
using GoVoylo.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
namespace GoVoylo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        Env.Load();

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
        builder.Services.AddScoped<IOtpRepository, OtpRepository>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

        // Registers MediatR and scans your Application project for Handlers
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GoVoylo.Application.Features.Payments.Commands.ProcessPayment.ProcessPaymentCommand).Assembly));

        builder.Services.AddValidatorsFromAssembly(
    typeof(RegisterUserCommandValidator).Assembly);

        builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        var app = builder.Build();
        // --- 4. HTTP PIPELINE MIDDLEWARE CONFIGURATION ---
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "GoVoylo API v1");
                options.RoutePrefix = "swagger"; // Access it via http://localhost:xxxx/swagger
            });
        }

        // Enable the heavy traffic protection middleware
        app.UseRateLimiter();

        app.UseHttpsRedirection();
        app.UseCors("AllowReactApp");
        app.UseAuthorization();

        // Map your controllers so the API routing endpoints actually work
        app.MapControllers();

        app.Run();
    }

}
