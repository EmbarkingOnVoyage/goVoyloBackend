using GoVoylo.Application.Features.Pricing.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Domain.Pricing;

namespace GoVoylo.Application.Features.Pricing.Services
{
    public class ConvenienceFeeService : IConvenienceFeeService
    {
        private readonly IConvenienceFeeRuleRepository _ruleRepository;

        public ConvenienceFeeService(IConvenienceFeeRuleRepository ruleRepository)
        {
            _ruleRepository = ruleRepository;
        }

        public async Task<ConvenienceFeeRulesDto> GetRulesAsync(CancellationToken cancellationToken)
        {
            var tripRates = await _ruleRepository.GetActiveTripRatesAsync(cancellationToken);
            var paxBands = await _ruleRepository.GetActivePaxBandsAsync(cancellationToken);

            return new ConvenienceFeeRulesDto(
                tripRates
                    .Select(r => new ConvenienceFeeTripRateDto(r.TripType, r.FirstJourneyPercent, r.ExtraJourneyPercent, r.MaxFeePerPax))
                    .ToList(),
                paxBands.Select(b => new ConvenienceFeePaxBandDto(b.MinPax, b.Factor)).ToList());
        }

        public async Task<decimal> CalculateAsync(
            string tripType,
            IReadOnlyList<JourneyBaseFare> journeys,
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
