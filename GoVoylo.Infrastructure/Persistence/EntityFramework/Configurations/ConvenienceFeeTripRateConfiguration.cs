using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class ConvenienceFeeTripRateConfiguration : IEntityTypeConfiguration<ConvenienceFeeTripRate>
    {
        private static readonly DateTime SeededAt = new(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc);

        public void Configure(EntityTypeBuilder<ConvenienceFeeTripRate> builder)
        {
            builder.ToTable("gv_convenience_fee_trip_rates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.TripType)
                .HasColumnName("trip_type")
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.TripType)
                .IsUnique()
                .HasDatabaseName("ux_convenience_fee_trip_rates_trip_type");

            builder.Property(x => x.FirstJourneyPercent)
                .HasColumnName("first_journey_percent")
                .HasColumnType("numeric(6,3)")
                .IsRequired();

            builder.Property(x => x.ExtraJourneyPercent)
                .HasColumnName("extra_journey_percent")
                .HasColumnType("numeric(6,3)")
                .IsRequired();

            builder.Property(x => x.MaxFeePerPax)
                .HasColumnName("max_fee_per_pax")
                .HasColumnType("numeric(12,2)");

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Fixed IDs so seeded data stays stable across environments/migrations
            builder.HasData(
                new
                {
                    Id = Guid.Parse("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c01"), TripType = TripTypes.OneWay,
                    FirstJourneyPercent = 1.00m, ExtraJourneyPercent = 1.00m, IsActive = true,
                    CreatedAt = SeededAt, UpdatedAt = SeededAt
                },
                new
                {
                    Id = Guid.Parse("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c02"), TripType = TripTypes.RoundTrip,
                    FirstJourneyPercent = 0.80m, ExtraJourneyPercent = 0.80m, IsActive = true,
                    CreatedAt = SeededAt, UpdatedAt = SeededAt
                },
                new
                {
                    Id = Guid.Parse("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c03"), TripType = TripTypes.MultiCity,
                    FirstJourneyPercent = 1.00m, ExtraJourneyPercent = 0.75m, IsActive = true,
                    CreatedAt = SeededAt, UpdatedAt = SeededAt
                });
        }
    }
}
