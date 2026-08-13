using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class SavedTravelerConfiguration : IEntityTypeConfiguration<SavedTraveler>
    {
        public void Configure(EntityTypeBuilder<SavedTraveler> builder)
        {
            builder.ToTable("gv_saved_travelers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.TravelerType)
                .HasColumnName("traveler_type")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DateOfBirth)
                .HasColumnName("date_of_birth")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Gender)
                .HasColumnName("gender")
                .HasMaxLength(10);

            builder.Property(x => x.Nationality)
                .HasColumnName("nationality")
                .HasMaxLength(50);

            builder.Property(x => x.MealPreference)
                .HasColumnName("meal_preference")
                .HasMaxLength(30);

            builder.Property(x => x.SeatPreference)
                .HasColumnName("seat_preference")
                .HasMaxLength(10);

            builder.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
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

            builder.HasIndex(x => x.UserId)
                .HasFilter("NOT is_deleted")
                .HasDatabaseName("ix_saved_travelers_user");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
