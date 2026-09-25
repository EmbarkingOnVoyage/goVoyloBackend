using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class TripBookingConfiguration : IEntityTypeConfiguration<TripBooking>
    {
        public void Configure(EntityTypeBuilder<TripBooking> builder)
        {
            builder.ToTable("gv_trip_bookings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.BookingRefNo)
                .HasColumnName("booking_ref_no")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.AirlinePnr)
                .HasColumnName("airline_pnr")
                .HasMaxLength(32);

            builder.Property(x => x.CrsPnr)
                .HasColumnName("crs_pnr")
                .HasMaxLength(32);

            builder.Property(x => x.RecordLocator)
                .HasColumnName("record_locator")
                .HasMaxLength(32);

            builder.Property(x => x.StatusId)
                .HasColumnName("status_id")
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.LocalStatus)
                .HasColumnName("local_status")
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            builder.Property(x => x.CurrencyCode)
                .HasColumnName("currency_code")
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.PassengerNames)
                .HasColumnName("passenger_names")
                .IsRequired();

            builder.Property(x => x.PaxIds)
                .HasColumnName("pax_ids")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.CancelledAt)
                .HasColumnName("cancelled_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.CancellationType)
                .HasColumnName("cancellation_type");

            builder.Property(x => x.CancelCode)
                .HasColumnName("cancel_code")
                .HasMaxLength(8);

            builder.Ignore(x => x.Legs);

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("ix_trip_bookings_user_id");
        }
    }
}
