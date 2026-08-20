using System.Text.Json;
using GoVoylo.Application.Common;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.Jobs
{
    public class PassportExpiryAlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PassportExpiryAlertOptions _options;
        private readonly ILogger<PassportExpiryAlertBackgroundService> _logger;

        public PassportExpiryAlertBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<PassportExpiryAlertOptions> options,
            ILogger<PassportExpiryAlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.RunIntervalHours));

            // Run once immediately on startup, then on the configured interval —
            // otherwise nothing would fire until the first interval had elapsed.
            do
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Passport expiry alert run failed.");
                }
            }
            while (!stoppingToken.IsCancellationRequested
                && await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var travelerPassportRepository = scope.ServiceProvider.GetRequiredService<ITravelerPassportRepository>();
            var savedTravelerRepository = scope.ServiceProvider.GetRequiredService<ISavedTravelerRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var activityLogRepository = scope.ServiceProvider.GetRequiredService<IActivityLogRepository>();

            var windowEnd = DateTime.UtcNow.Date.AddDays(_options.WindowDays);

            await AlertForTravelerPassportsAsync(
                travelerPassportRepository, savedTravelerRepository, userRepository,
                encryptionService, emailService, activityLogRepository, windowEnd, cancellationToken);

            await AlertForOwnPassportsAsync(
                userRepository, encryptionService, emailService, activityLogRepository, windowEnd, cancellationToken);
        }

        private async Task AlertForTravelerPassportsAsync(
            ITravelerPassportRepository travelerPassportRepository,
            ISavedTravelerRepository savedTravelerRepository,
            IUserRepository userRepository,
            IEncryptionService encryptionService,
            IEmailService emailService,
            IActivityLogRepository activityLogRepository,
            DateTime windowEnd,
            CancellationToken cancellationToken)
        {
            var passports = await travelerPassportRepository.GetExpiringUnnotifiedAsync(windowEnd);

            foreach (var passport in passports)
            {
                try
                {
                    var traveler = await savedTravelerRepository.GetByIdAsync(passport.SavedTravelerId);
                    if (traveler == null)
                    {
                        continue;
                    }

                    var user = await userRepository.GetByIdAsync(traveler.UserId);
                    if (user?.Email == null)
                    {
                        continue;
                    }

                    var travelerName = $"{traveler.FirstName} {traveler.LastName}";
                    var masked = MaskingHelper.MaskKeepLast4(
                        encryptionService.Decrypt(passport.PassportNumberEncrypted));

                    // Email and in-app are independent channels — an SMTP failure
                    // shouldn't suppress the in-app alert or block the dedup marker.
                    try
                    {
                        await emailService.SendPassportExpiryAlertAsync(
                            user.Email, travelerName, masked, passport.ExpiryDate);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(
                            emailEx, "Failed to email passport expiry alert for traveler passport {PassportId}",
                            passport.Id);
                    }

                    var payload = JsonSerializer.Serialize(new
                    {
                        travelerId = traveler.Id,
                        travelerName,
                        maskedPassportNumber = masked,
                        expiryDate = passport.ExpiryDate.ToString("yyyy-MM-dd")
                    });

                    await activityLogRepository.LogActivityAsync(
                        new UserActivityLog(user.Id.ToString(), "PassportExpiryAlert", payload, "system"),
                        cancellationToken);

                    passport.MarkExpiryAlertSent();
                    await travelerPassportRepository.UpdateAsync(passport);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex, "Failed to send passport expiry alert for traveler passport {PassportId}", passport.Id);
                }
            }
        }

        private async Task AlertForOwnPassportsAsync(
            IUserRepository userRepository,
            IEncryptionService encryptionService,
            IEmailService emailService,
            IActivityLogRepository activityLogRepository,
            DateTime windowEnd,
            CancellationToken cancellationToken)
        {
            var users = await userRepository.GetWithExpiringPassportUnnotifiedAsync(windowEnd);

            foreach (var user in users)
            {
                try
                {
                    if (user.Email == null || user.PassportNumberEncrypted == null
                        || user.PassportExpiryDate == null)
                    {
                        continue;
                    }

                    var name = $"{user.FirstName} {user.LastName}";
                    var masked = MaskingHelper.MaskKeepLast4(
                        encryptionService.Decrypt(user.PassportNumberEncrypted));

                    try
                    {
                        await emailService.SendPassportExpiryAlertAsync(
                            user.Email, name, masked, user.PassportExpiryDate.Value);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(
                            emailEx, "Failed to email passport expiry alert for user {UserId}", user.Id);
                    }

                    var payload = JsonSerializer.Serialize(new
                    {
                        maskedPassportNumber = masked,
                        expiryDate = user.PassportExpiryDate.Value.ToString("yyyy-MM-dd")
                    });

                    await activityLogRepository.LogActivityAsync(
                        new UserActivityLog(user.Id.ToString(), "PassportExpiryAlert", payload, "system"),
                        cancellationToken);

                    user.MarkPassportExpiryAlertSent();
                    await userRepository.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send passport expiry alert for user {UserId}", user.Id);
                }
            }
        }
    }
}
