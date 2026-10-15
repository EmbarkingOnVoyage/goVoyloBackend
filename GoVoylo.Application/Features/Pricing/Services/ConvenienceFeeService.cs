using GoVoylo.Application.Features.Pricing.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Domain.Pricing;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Application.Features.Pricing.Services
{
    public class ConvenienceFeeService : IConvenienceFeeService
    {
        private readonly IConvenienceFeeRuleRepository _ruleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly IConfiguration _configuration;

        public ConvenienceFeeService(
            IConvenienceFeeRuleRepository ruleRepository,
            IUserRepository userRepository,
            ITripBookingRepository tripBookingRepository,
            IConfiguration configuration)
        {
            _ruleRepository = ruleRepository;
            _userRepository = userRepository;
            _tripBookingRepository = tripBookingRepository;
            _configuration = configuration;
        }

        // The offer ends at ConvenienceFee:FirstBookingFreeUntil (an app setting,
        // e.g. 2026-11-15T23:59:59+05:30); unset or unparsable means no offer. Only
        // a paid booking uses it up, so an abandoned or released hold doesn't.
        public async Task<ConvenienceFeeOfferDto> GetOfferAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (!DateTimeOffset.TryParse(_configuration["ConvenienceFee:FirstBookingFreeUntil"], out var until)
                || DateTimeOffset.UtcNow > until)
            {
                return new ConvenienceFeeOfferDto(false, null);
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsGuest)
            {
                return new ConvenienceFeeOfferDto(false, until);
            }

            var bookings = await _tripBookingRepository.GetByUserIdAsync(userId, cancellationToken);
            return new ConvenienceFeeOfferDto(bookings.All(b => b.AmountPaid == null), until);
        }

        public async Task<ConvenienceFeeRulesDto> GetRulesAsync(CancellationToken cancellationToken)
        {
            var tripRates = await _ruleRepository.GetActiveTripRatesAsync(cancellationToken);
            var paxBands = await _ruleRepository.GetActivePaxBandsAsync(cancellationToken);

            return new ConvenienceFeeRulesDto(
                tripRates
                    .Select(r => new ConvenienceFeeTripRateDto(
                        r.TripType, r.FirstJourneyPercent, r.ExtraJourneyPercent, r.MaxFeePerPax, r.FullRateJourneys, r.ApplyPaxBands))
                    .ToList(),
                paxBands.Select(b => new ConvenienceFeePaxBandDto(b.MinPax, b.Factor)).ToList());
        }

        public async Task<decimal> CalculateAsync(
            string tripType,
            IReadOnlyList<JourneyFare> journeys,
            int adultCount,
            int childCount,
            CancellationToken cancellationToken)
        {
            var tripRates = await _ruleRepository.GetActiveTripRatesAsync(cancellationToken);
            var tripRate = tripRates.FirstOrDefault(r => string.Equals(r.TripType, tripType, StringComparison.OrdinalIgnoreCase));
            if (tripRate == null)
            {
                return 0m;
            }

            var paxBands = await _ruleRepository.GetActivePaxBandsAsync(cancellationToken);
            return ConvenienceFeeCalculator.Calculate(tripRate, paxBands, journeys, adultCount, childCount);
        }
    }
}
