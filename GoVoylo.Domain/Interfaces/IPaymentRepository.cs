using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task SaveAsync(PaymentTransaction transaction, CancellationToken cancellationToken);
        Task<PaymentTransaction?> GetByIdAsync(Guid id);

        Task<BookingPayment?> GetByReferenceAsync(string bookingReference);
        Task SaveAsync(BookingPayment payment);
        Task<BookingPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}