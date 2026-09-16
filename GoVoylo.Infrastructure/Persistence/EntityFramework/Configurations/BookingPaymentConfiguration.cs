using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoVoylo.Domain.Entities;

namespace GoVoyis.Infrastructure.Persistence.EntityFramework.Configurations;

public class BookingPaymentConfiguration : IEntityTypeConfiguration<BookingPayment>
{
    public void Configure(EntityTypeBuilder<BookingPayment> builder)
    {
        // 1. Define Table Name (Optional: maps explicitly to PostgreSQL)
        builder.ToTable("BookingPayments");

        // 2. Primary Key
        builder.HasKey(e => e.Id);

        // 3. Column Properties
        builder.Property(e => e.BookingReference)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);
    }
}
