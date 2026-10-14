using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class AccessAuditLogConfiguration : IEntityTypeConfiguration<AccessAuditLog>
    {
        public void Configure(EntityTypeBuilder<AccessAuditLog> builder)
        {
            builder.ToTable("gv_access_audit_logs");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(20).IsRequired();
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.SignInMethod).HasColumnName("sign_in_method").HasMaxLength(20);

            builder.Property(x => x.BookingReference).HasColumnName("booking_reference").HasMaxLength(100);
            builder.Property(x => x.GatewayOrderId).HasColumnName("gateway_order_id").HasMaxLength(100);
            builder.Property(x => x.GatewayPaymentId).HasColumnName("gateway_payment_id").HasMaxLength(100);
            builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)");
            builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
            builder.Property(x => x.PaymentStatus).HasColumnName("payment_status").HasMaxLength(20);
            builder.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(30);
            builder.Property(x => x.PaymentMethodDetail).HasColumnName("payment_method_detail").HasMaxLength(150);

            builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            builder.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);

            builder.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
            builder.Property(x => x.Region).HasColumnName("region").HasMaxLength(100);
            builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2);
            builder.Property(x => x.Isp).HasColumnName("isp").HasMaxLength(150);
            builder.Property(x => x.Latitude).HasColumnName("latitude").HasColumnType("numeric(9,6)");
            builder.Property(x => x.Longitude).HasColumnName("longitude").HasColumnType("numeric(9,6)");

            builder.Property(x => x.IsVpn).HasColumnName("is_vpn");
            builder.Property(x => x.IsProxy).HasColumnName("is_proxy");
            builder.Property(x => x.IsTor).HasColumnName("is_tor");
            builder.Property(x => x.RecentAbuse).HasColumnName("recent_abuse");
            builder.Property(x => x.IsBot).HasColumnName("is_bot");
            builder.Property(x => x.FraudScore).HasColumnName("fraud_score");
            builder.Property(x => x.IsSecure).HasColumnName("is_secure");
            builder.Property(x => x.LookupStatus).HasColumnName("lookup_status").HasMaxLength(20).IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

            builder.HasIndex(x => x.UserId).HasDatabaseName("ix_access_audit_logs_user");
            builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_access_audit_logs_created_at");
            builder.HasIndex(x => x.GatewayOrderId).HasDatabaseName("ix_access_audit_logs_gateway_order");
            builder.HasIndex(x => new { x.IpAddress, x.CreatedAt }).HasDatabaseName("ix_access_audit_logs_ip");

            // Loose user reference, like gv_audit_logs: the trail outlives the user row.
        }
    }
}
