using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class CustomerGstDetailConfiguration : IEntityTypeConfiguration<CustomerGstDetail>
    {
        public void Configure(EntityTypeBuilder<CustomerGstDetail> builder)
        {
            builder.ToTable("gv_customer_gst_details");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.Gstin)
                .HasColumnName("gstin")
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.LegalName)
                .HasColumnName("legal_name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.TradeName)
                .HasColumnName("trade_name")
                .HasMaxLength(255);

            builder.Property(x => x.IsVerified)
                .HasColumnName("is_verified")
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

            builder.HasIndex(x => x.Gstin)
                .IsUnique()
                .HasDatabaseName("ux_customer_gst_gstin");

            // Singleton per user for MVP (PUT /customer/gst has no {id}); ticket 016
            // flags "multiple GST profiles" as a future item — drop this index if that ships.
            builder.HasIndex(x => x.UserId)
                .IsUnique()
                .HasDatabaseName("ux_customer_gst_one_per_user");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
