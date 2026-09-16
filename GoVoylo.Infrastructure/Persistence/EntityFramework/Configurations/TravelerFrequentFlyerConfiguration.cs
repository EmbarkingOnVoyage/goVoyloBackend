using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class TravelerFrequentFlyerConfiguration : IEntityTypeConfiguration<TravelerFrequentFlyer>
    {
        public void Configure(EntityTypeBuilder<TravelerFrequentFlyer> builder)
        {
            builder.ToTable("gv_traveler_frequent_flyers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.SavedTravelerId)
                .HasColumnName("saved_traveler_id")
                .IsRequired();

            builder.Property(x => x.AirlineCode)
                .HasColumnName("airline_code")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.MembershipNumberEncrypted)
                .HasColumnName("membership_number_encrypted")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => new { x.SavedTravelerId, x.AirlineCode })
                .IsUnique()
                .HasDatabaseName("ux_traveler_frequent_flyer_airline");

            builder.HasOne<SavedTraveler>()
                .WithMany()
                .HasForeignKey(x => x.SavedTravelerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
