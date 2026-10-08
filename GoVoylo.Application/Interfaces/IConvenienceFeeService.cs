using GoVoylo.Application.Features.Pricing.Dtos;
using GoVoylo.Domain.Pricing;

namespace GoVoylo.Application.Interfaces
{
    public interface IConvenienceFeeService
    {
        Task<ConvenienceFeeRulesDto> GetRulesAsync(CancellationToken cancellationToken);

        // 0 when no active rate exists for the trip type.
        Task<decimal> CalculateAsync(
            string tripType,
            IReadOnlyList<JourneyBaseFare> journeys,
            int adultCount,
            int childCount,
            CancellationToken cancellationToken);
    }
}
