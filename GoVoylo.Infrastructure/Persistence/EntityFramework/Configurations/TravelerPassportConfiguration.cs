using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class TravelerPassportConfiguration : IEntityTypeConfiguration<TravelerPassport>
    {
        public void Configure(EntityTypeBuilder<TravelerPassport> builder)
        {
            builder.ToTable("gv_traveler_passports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.SavedTravelerId)
                .HasColumnName("saved_traveler_id")
                .IsRequired();

            builder.Property(x => x.PassportNumberEncrypted)
                .HasColumnName("passport_number_encrypted")
                .IsRequired();

            builder.Property(x => x.IssuingCountry)
                .HasColumnName("issuing_country")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ExpiryDate)
                .HasColumnName("expiry_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.LastExpiryAlertSentAt)
                .HasColumnName("last_expiry_alert_sent_at")
                .HasColumnType("timestamptz");

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

            builder.HasIndex(x => x.SavedTravelerId)
                .IsUnique()
                .HasDatabaseName("ux_traveler_passport");

            builder.HasOne<SavedTraveler>()
                .WithMany()
                .HasForeignKey(x => x.SavedTravelerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
