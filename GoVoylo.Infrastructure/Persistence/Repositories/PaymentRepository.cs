// GoVoylo.Infrastructure/Persistence/Repositories/PaymentRepository.cs
using Microsoft.EntityFrameworkCore;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;

namespace GoVoylo.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingPayment?> GetByReferenceAsync(string bookingReference)
    {
        return await _context.BookingPayments
            .FirstOrDefaultAsync(p => p.BookingReference == bookingReference);
    }

    public async Task SaveAsync(BookingPayment payment)
    {
        await _context.BookingPayments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync(PaymentTransaction transaction, CancellationToken cancellationToken)
    {
        // Simulate saving the transaction to a database or external system
        await Task.Delay(100, cancellationToken); // Simulated async operation
    }

    public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
    {
        // Simulate retrieving the transaction from a database or external system
        await Task.Delay(100); // Simulated async operation
        return null; // For demonstration purposes, returning null
    }
}
