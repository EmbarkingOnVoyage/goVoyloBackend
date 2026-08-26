using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class TravelerVisaConfiguration : IEntityTypeConfiguration<TravelerVisa>
    {
        public void Configure(EntityTypeBuilder<TravelerVisa> builder)
        {
            builder.ToTable("gv_traveler_visas");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.SavedTravelerId)
                .HasColumnName("saved_traveler_id")
                .IsRequired();

            builder.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.VisaNumberEncrypted)
                .HasColumnName("visa_number_encrypted")
                .IsRequired();

            builder.Property(x => x.VisaType)
                .HasColumnName("visa_type")
                .HasMaxLength(30);

            builder.Property(x => x.IssueDate)
                .HasColumnName("issue_date")
                .HasColumnType("date");

            builder.Property(x => x.ExpiryDate)
                .HasColumnName("expiry_date")
                .HasColumnType("date")
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

            builder.HasIndex(x => new { x.SavedTravelerId, x.Country })
                .IsUnique()
                .HasDatabaseName("ux_traveler_visa_country");

            builder.HasOne<SavedTraveler>()
                .WithMany()
                .HasForeignKey(x => x.SavedTravelerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
