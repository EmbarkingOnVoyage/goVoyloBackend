using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IConvenienceFeeRuleRepository
    {
        Task<IReadOnlyList<ConvenienceFeeTripRate>> GetActiveTripRatesAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<ConvenienceFeePaxBand>> GetActivePaxBandsAsync(CancellationToken cancellationToken);
    }
}
