using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.Jobs
{
    // Releases holds whose checkout was abandoned without a release call (the
    // tab or app closed while Razorpay was open), so My Trips doesn't keep
    // showing them as on hold. The supplier release is best-effort — an unpaid
    // hold lapses with the airline on its own anyway — and the booking is marked
    // Released either way.
    public class UnpaidHoldExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UnpaidHoldExpiryOptions _options;
        private readonly ILogger<UnpaidHoldExpiryBackgroundService> _logger;

        public UnpaidHoldExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<UnpaidHoldExpiryOptions> options,
            ILogger<UnpaidHoldExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.RunIntervalMinutes));

            do
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unpaid hold expiry run failed.");
                }
            }
            while (!stoppingToken.IsCancellationRequested
                && await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var tripBookingRepository = scope.ServiceProvider.GetRequiredService<ITripBookingRepository>();
            var supplierClientResolver = scope.ServiceProvider.GetRequiredService<IFlightSupplierClientResolver>();

            var cutoff = DateTime.UtcNow.AddMinutes(-_options.UnpaidHoldMinutes);
            var holds = await tripBookingRepository.GetUnpaidHoldsCreatedBeforeAsync(cutoff, cancellationToken);

            foreach (var booking in holds)
            {
                if (!string.IsNullOrWhiteSpace(booking.AirlinePnr))
                {
                    try
                    {
                        await supplierClientResolver.Resolve(booking.SupplierCode).ReleaseHoldAsync(
                            new SupplierReleaseHoldRequestDto(booking.BookingRefNo, booking.AirlinePnr),
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Supplier release failed for abandoned hold {BookingRefNo}; marking it released anyway.",
                            booking.BookingRefNo);
                    }
                }

                booking.MarkReleased();
                await tripBookingRepository.UpdateAsync(booking, cancellationToken);
                _logger.LogInformation("Released abandoned unpaid hold {BookingRefNo}.", booking.BookingRefNo);
            }
        }
    }
}
