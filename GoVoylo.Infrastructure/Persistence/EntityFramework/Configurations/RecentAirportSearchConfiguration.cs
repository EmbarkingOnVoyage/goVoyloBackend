using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class RecentAirportSearchConfiguration : IEntityTypeConfiguration<RecentAirportSearch>
    {
        public void Configure(EntityTypeBuilder<RecentAirportSearch> builder)
        {
            builder.ToTable("gv_recent_airport_searches");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.IataCode)
                .HasColumnName("iata_code")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.SearchedAt)
                .HasColumnName("searched_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => new { x.UserId, x.IataCode })
                .IsUnique()
                .HasDatabaseName("ux_recent_airport_searches_user_iata");

            builder.HasIndex(x => new { x.UserId, x.SearchedAt })
                .HasDatabaseName("ix_recent_airport_searches_user_recency");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
