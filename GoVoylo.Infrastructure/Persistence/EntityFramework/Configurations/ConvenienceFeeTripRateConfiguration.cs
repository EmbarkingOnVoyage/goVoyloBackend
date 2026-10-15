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

            builder.Property(x => x.FullRateJourneys)
                .HasColumnName("full_rate_journeys")
                .HasDefaultValue(1)
                .IsRequired();

            // No database default: with one of true, EF could never seed or insert false.
            builder.Property(x => x.ApplyPaxBands)
                .HasColumnName("apply_pax_bands")
                .IsRequired();

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
                    FirstJourneyPercent = 2.64m, ExtraJourneyPercent = 2.64m, FullRateJourneys = 1, ApplyPaxBands = true,
                    IsActive = true,
                    CreatedAt = SeededAt, UpdatedAt = SeededAt
                },
                new
                {
                    Id = Guid.Parse("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c02"), TripType = TripTypes.RoundTrip,
                    FirstJourneyPercent = 2.64m, ExtraJourneyPercent = 2.64m, FullRateJourneys = 2, ApplyPaxBands = true,
                    IsActive = true,
                    CreatedAt = SeededAt, UpdatedAt = SeededAt
                },
                new
                {
                    Id = Guid.Parse("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c03"), TripType = TripTypes.MultiCity,
                    // First two journeys at 2.64%, the rest at 75% of that; no group factor.
                    FirstJourneyPercent = 2.64m, ExtraJourneyPercent = 1.98m, FullRateJourneys = 2, ApplyPaxBands = false,
                    IsActive = true,
                    CreatedAt = SeededAt, UpdatedAt = SeededAt
                });
        }
    }
}
