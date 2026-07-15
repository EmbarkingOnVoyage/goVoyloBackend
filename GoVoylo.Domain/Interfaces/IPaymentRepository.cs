using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task SaveAsync(PaymentTransaction transaction, CancellationToken cancellationToken);
        Task<PaymentTransaction?> GetByIdAsync(Guid id);
    }
}