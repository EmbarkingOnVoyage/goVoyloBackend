using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("gv_audit_logs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("ix_audit_logs_user");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("ix_audit_logs_created_at");

            // Loose reference, not a real FK: audit history must survive even if
            // the user row it references is ever hard-deleted (users are only
            // soft-deleted today, but the audit trail shouldn't depend on that).
        }
    }
}
