using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.ToTable("gv_notification_preferences", x =>
            {
                x.HasCheckConstraint(
                    "chk_one_channel_enabled",
                    "email_transactional OR sms_transactional OR push_enabled");
            });

            builder.HasKey(x => x.UserId);

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.EmailTransactional)
                .HasColumnName("email_transactional")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.EmailMarketing)
                .HasColumnName("email_marketing")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.SmsTransactional)
                .HasColumnName("sms_transactional")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.SmsMarketing)
                .HasColumnName("sms_marketing")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.PushEnabled)
                .HasColumnName("push_enabled")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
