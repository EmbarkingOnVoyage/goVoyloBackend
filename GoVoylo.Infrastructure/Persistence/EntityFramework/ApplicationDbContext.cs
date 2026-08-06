// GoVoylo.Infrastructure/Persistence/EntityFramework/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using GoVoylo.Domain.Entities;
using GoVoylo.Infrastructure.Services;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<BookingPayment> BookingPayments => Set<BookingPayment>();
    public DbSet<FlightBooking> FlightBookings => Set<FlightBooking>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Define PostgreSQL database rules cleanly
        modelBuilder.Entity<BookingPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingReference).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
        });
    }
}
