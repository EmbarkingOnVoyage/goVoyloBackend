using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class CommissionDiscountRateConfiguration : IEntityTypeConfiguration<CommissionDiscountRate>
    {
        private static readonly DateTime SeededAt = new(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);

        public void Configure(EntityTypeBuilder<CommissionDiscountRate> builder)
        {
            builder.ToTable("gv_commission_discount_rates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.Percent)
                .HasColumnName("percent")
                .HasColumnType("numeric(6,3)")
                .IsRequired();

            builder.Property(x => x.EffectiveFrom)
                .HasColumnName("effective_from")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasIndex(x => x.EffectiveFrom)
                .HasDatabaseName("ix_commission_discount_rates_effective_from");

            builder.Property(x => x.Note)
                .HasColumnName("note")
                .HasMaxLength(200);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Launch offer: the whole commission is given back as discount, until a
            // newer row says otherwise (planned review after 15 Nov 2026).
            builder.HasData(new
            {
                Id = Guid.Parse("3c1d7b2e-5f4a-4e8b-9c62-0a7d1e4b9f01"),
                Percent = 100.000m,
                EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Note = "Launch offer: 100% of commission as discount (until at least 15 Nov 2026)",
                CreatedAt = SeededAt
            });
        }
    }
}
