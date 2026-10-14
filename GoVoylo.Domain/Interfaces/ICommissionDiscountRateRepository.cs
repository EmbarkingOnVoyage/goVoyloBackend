namespace GoVoylo.Domain.Interfaces
{
    public interface ICommissionDiscountRateRepository
    {
        // Percent (0–100) of the commission given as discount at the given moment.
        Task<decimal> GetPercentAtAsync(DateTime utc, CancellationToken cancellationToken);
    }
}
