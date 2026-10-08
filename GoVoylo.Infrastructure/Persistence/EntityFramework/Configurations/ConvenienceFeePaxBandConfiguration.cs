using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class ConvenienceFeePaxBandConfiguration : IEntityTypeConfiguration<ConvenienceFeePaxBand>
    {
        private static readonly DateTime SeededAt = new(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc);

        public void Configure(EntityTypeBuilder<ConvenienceFeePaxBand> builder)
        {
            builder.ToTable("gv_convenience_fee_pax_bands");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.MinPax)
                .HasColumnName("min_pax")
                .IsRequired();

            builder.HasIndex(x => x.MinPax)
                .IsUnique()
                .HasDatabaseName("ux_convenience_fee_pax_bands_min_pax");

            builder.Property(x => x.Factor)
                .HasColumnName("factor")
                .HasColumnType("numeric(6,3)")
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
                new { Id = Guid.Parse("8a3d5b2e-7c1f-4e9a-b604-3c2e1f9d8a01"), MinPax = 1, Factor = 1.00m, IsActive = true, CreatedAt = SeededAt, UpdatedAt = SeededAt },
                new { Id = Guid.Parse("8a3d5b2e-7c1f-4e9a-b604-3c2e1f9d8a02"), MinPax = 3, Factor = 0.85m, IsActive = true, CreatedAt = SeededAt, UpdatedAt = SeededAt },
                new { Id = Guid.Parse("8a3d5b2e-7c1f-4e9a-b604-3c2e1f9d8a03"), MinPax = 6, Factor = 0.75m, IsActive = true, CreatedAt = SeededAt, UpdatedAt = SeededAt });
        }
    }
}
