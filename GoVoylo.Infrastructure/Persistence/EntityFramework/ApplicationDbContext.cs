// GoVoylo.Infrastructure/Persistence/EntityFramework/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<BookingPayment> BookingPayments => Set<BookingPayment>();
    public DbSet<FlightBooking> FlightBookings => Set<FlightBooking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
