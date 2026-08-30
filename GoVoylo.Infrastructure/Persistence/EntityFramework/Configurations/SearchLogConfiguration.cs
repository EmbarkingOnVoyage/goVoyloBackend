using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class SearchLogConfiguration : IEntityTypeConfiguration<SearchLog>
    {
        public void Configure(EntityTypeBuilder<SearchLog> builder)
        {
            builder.ToTable("gv_search_logs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.Origin)
                .HasColumnName("origin")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Destination)
                .HasColumnName("destination")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.TravelDate)
                .HasColumnName("travel_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.TripType)
                .HasColumnName("trip_type")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CabinClass)
                .HasColumnName("cabin_class")
                .HasMaxLength(20)
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

            builder.HasIndex(x => new { x.UserId, x.SearchedAt })
                .HasDatabaseName("ix_search_logs_user_recency");

            builder.HasIndex(x => new { x.Origin, x.Destination })
                .HasDatabaseName("ix_search_logs_route");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
