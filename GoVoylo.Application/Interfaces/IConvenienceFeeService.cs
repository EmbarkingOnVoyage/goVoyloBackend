using GoVoylo.Application.Features.Pricing.Dtos;
using GoVoylo.Domain.Pricing;

namespace GoVoylo.Application.Interfaces
{
    public interface IConvenienceFeeService
    {
        Task<ConvenienceFeeRulesDto> GetRulesAsync(CancellationToken cancellationToken);

        // Launch offer: a signed-in customer's first paid booking has no
        // convenience fee until ConvenienceFee:FirstBookingFreeUntil. Guests never
        // qualify (each guest checkout is a new guest user).
        Task<ConvenienceFeeOfferDto> GetOfferAsync(Guid userId, CancellationToken cancellationToken);

        // 0 when no active rate exists for the trip type.
        Task<decimal> CalculateAsync(
            string tripType,
            IReadOnlyList<JourneyFare> journeys,
            int adultCount,
            int childCount,
            CancellationToken cancellationToken);
    }
}
