using DotNetEnv;
using FluentValidation;
using GoVoylo.Api.Middleware;
using GoVoylo.Api.Services;
using GoVoylo.Application.Common.Behaviors;
using GoVoylo.Application.Features.Authentication.Commands.Register;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure;
using GoVoylo.Infrastructure.Caching;
using GoVoylo.Infrastructure.ExternalServices.Tripjack;
using GoVoylo.Infrastructure.Jobs;
using GoVoylo.Infrastructure.Logging;
using GoVoylo.Infrastructure.Monitoring;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using GoVoylo.Infrastructure.Persistence.Repositories;
using GoVoylo.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
namespace GoVoylo.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        // --- 1. EXISTING TEMPLATE SERVICES ---
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET is not configured.");

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the accessToken from /api/auth/login (no 'Bearer ' prefix needed here)."
                };
                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
                });
                return Task.CompletedTask;
            });
        });
        builder.Services.AddControllers(); // Add this line so .NET finds your PaymentsController

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

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
        builder.Services.AddScoped<IProfileImageStorageService, LocalProfileImageStorageService>();
        builder.Services.AddScoped<IUserPreferenceRepository, PreferenceRepository>();
        builder.Services.AddScoped<INotificationPreferenceRepository, PreferenceRepository>();
        builder.Services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
        builder.Services.AddScoped<ICustomerGstDetailRepository, CustomerGstDetailRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        builder.Services.AddScoped<IEncryptionService, AesGcmEncryptionService>();
        builder.Services.AddScoped<ISavedTravelerRepository, SavedTravelerRepository>();
        builder.Services.AddScoped<ITravelerPassportRepository, TravelerPassportRepository>();
        builder.Services.AddScoped<ITravelerVisaRepository, TravelerVisaRepository>();
        builder.Services.AddScoped<ITravelerFrequentFlyerRepository, TravelerFrequentFlyerRepository>();
        builder.Services.AddScoped<ITravelerSpecialAssistanceRepository, TravelerSpecialAssistanceRepository>();
        builder.Services.AddScoped<ITravelerEmergencyContactRepository, TravelerEmergencyContactRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        builder.Services.AddScoped<IAirportRepository, AirportRepository>();
        builder.Services.AddScoped<IRecentAirportSearchRepository, RecentAirportSearchRepository>();
        builder.Services.AddSingleton<IAirportCacheService, AirportCacheService>();
        builder.Services.AddScoped<AirportImportService>();
        builder.Services.AddSingleton<AuditLogQueue>();
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        builder.Services.AddHostedService<AuditLogBackgroundService>();
        builder.Services.Configure<PassportExpiryAlertOptions>(
            builder.Configuration.GetSection("PassportExpiryAlert"));
        builder.Services.AddHostedService<PassportExpiryAlertBackgroundService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IFlightSearchSessionStore, InMemoryFlightSearchSessionStore>();
        builder.Services.Configure<TripjackOptions>(builder.Configuration.GetSection("TripjackSettings"));
        builder.Services.AddHttpClient<IFlightSupplierClient, TripjackClient>((sp, client) =>
        {
            var tripjackOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TripjackOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(tripjackOptions.BaseUrl))
            {
                client.BaseAddress = new Uri(tripjackOptions.BaseUrl);
            }
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database")
            .AddCheck<EmailServiceHealthCheck>("email");

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

        var app = builder.Build();

        using (var startupScope = app.Services.CreateScope())
        {
            var airportImportService = startupScope.ServiceProvider.GetRequiredService<AirportImportService>();
            await airportImportService.ImportIfEmptyAsync();
        }

        // --- 4. HTTP PIPELINE MIDDLEWARE CONFIGURATION ---
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "GoVoylo API v1");
                options.RoutePrefix = "swagger";
            });
        }

        // Enable the heavy traffic protection middleware
        app.UseRateLimiter();

        app.UseHttpsRedirection();

        var profileImagesRootPath = builder.Configuration["Storage:ProfileImages:RootPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "uploads", "profile-images");
        var profileImagesPublicBasePath = builder.Configuration["Storage:ProfileImages:PublicBasePath"]
            ?? "/uploads/profile-images";
        Directory.CreateDirectory(profileImagesRootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(profileImagesRootPath),
            RequestPath = profileImagesPublicBasePath
        });

        app.UseAuthentication();
        app.UseAuthorization();

        // Not [Authorize]-gated — load balancer/orchestrator probes don't carry a JWT.
        // Restricted to internal/private IPs instead (see InternalNetworkGuard).
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
        }).AddEndpointFilter(async (context, next) =>
        {
            if (!InternalNetworkGuard.IsInternal(context.HttpContext.Connection.RemoteIpAddress))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            return await next(context);
        });

        // Map your controllers so the API routing endpoints actually work
        app.MapControllers();

        await app.RunAsync();
    }

}
