using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class TripBookingLegConfiguration : IEntityTypeConfiguration<TripBookingLeg>
    {
        public void Configure(EntityTypeBuilder<TripBookingLeg> builder)
        {
            builder.ToTable("gv_trip_booking_legs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.TripBookingId)
                .HasColumnName("trip_booking_id")
                .IsRequired();

            builder.Property(x => x.LegIndex)
                .HasColumnName("leg_index")
                .IsRequired();

            builder.Property(x => x.Origin)
                .HasColumnName("origin")
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.Destination)
                .HasColumnName("destination")
                .HasMaxLength(8)
                .IsRequired();

            // Plain timestamp, not timestamptz: this is the flight's local departure
            // wall-clock time (parsed from Flyshop as Kind=Unspecified), not a UTC
            // instant — Npgsql refuses non-UTC DateTimes for timestamptz outright.
            builder.Property(x => x.TravelDate)
                .HasColumnName("travel_date")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(x => x.AirlineCode)
                .HasColumnName("airline_code")
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.AirlineName)
                .HasColumnName("airline_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FlightNumber)
                .HasColumnName("flight_number")
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(x => x.FlightId)
                .HasColumnName("flight_id")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.IsCancelled)
                .HasColumnName("is_cancelled")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.CancelledAt)
                .HasColumnName("cancelled_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.CancellationType)
                .HasColumnName("cancellation_type");

            builder.Property(x => x.CancelCode)
                .HasColumnName("cancel_code")
                .HasMaxLength(8);

            builder.HasOne<TripBooking>()
                .WithMany()
                .HasForeignKey(x => x.TripBookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.TripBookingId)
                .HasDatabaseName("ix_trip_booking_legs_trip_booking_id");
        }
    }
}
