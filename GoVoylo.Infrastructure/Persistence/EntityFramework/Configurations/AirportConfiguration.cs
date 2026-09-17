using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class AirportConfiguration : IEntityTypeConfiguration<Airport>
    {
        public void Configure(EntityTypeBuilder<Airport> builder)
        {
            builder.ToTable("gv_airports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.IataCode)
                .HasColumnName("iata_code")
                .HasMaxLength(3)
                .IsRequired();

            builder.HasIndex(x => x.IataCode)
                .IsUnique()
                .HasDatabaseName("ux_airports_iata_code");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.City)
                .HasDatabaseName("ix_airports_city");

            builder.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IsPopular)
                .HasColumnName("is_popular")
                .HasDefaultValue(false)
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
        }
    }
}
