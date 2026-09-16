using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
    {
        public void Configure(EntityTypeBuilder<CustomerAddress> builder)
        {
            builder.ToTable("gv_customer_addresses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.Label)
                .HasColumnName("label")
                .HasMaxLength(30);

            builder.Property(x => x.Line1)
                .HasColumnName("line1")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Line2)
                .HasColumnName("line2")
                .HasMaxLength(255);

            builder.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.State)
                .HasColumnName("state")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(12)
                .IsRequired();

            builder.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(2)
                .HasDefaultValue("IN")
                .IsRequired();

            builder.Property(x => x.IsDefault)
                .HasColumnName("is_default")
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
                .HasFilter("is_default")
                .IsUnique()
                .HasDatabaseName("ux_customer_addresses_one_default");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
