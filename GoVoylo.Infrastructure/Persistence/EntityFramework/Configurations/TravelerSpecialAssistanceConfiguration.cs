using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class TravelerSpecialAssistanceConfiguration : IEntityTypeConfiguration<TravelerSpecialAssistance>
    {
        public void Configure(EntityTypeBuilder<TravelerSpecialAssistance> builder)
        {
            builder.ToTable("gv_traveler_special_assistance");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.SavedTravelerId)
                .HasColumnName("saved_traveler_id")
                .IsRequired();

            builder.Property(x => x.SsrCode)
                .HasColumnName("ssr_code")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasColumnName("notes");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => new { x.SavedTravelerId, x.SsrCode })
                .IsUnique()
                .HasDatabaseName("ux_traveler_special_assistance_code");

            builder.HasOne<SavedTraveler>()
                .WithMany()
                .HasForeignKey(x => x.SavedTravelerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
