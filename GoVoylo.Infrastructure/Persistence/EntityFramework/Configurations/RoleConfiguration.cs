using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("gv_roles");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            // Name
            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            // Unique name
            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("ux_roles_name");

            // Fixed IDs so seeded data stays stable across environments/migrations
            builder.HasData(
                new { Id = Guid.Parse("4bbebc58-b75d-434d-bb5a-29c6bf7c8fe7"), Name = "customer" },
                new { Id = Guid.Parse("a04f3f16-299b-4087-a858-d12ca890794b"), Name = "support_agent" },
                new { Id = Guid.Parse("9ebf67c2-40b6-4481-b580-895303558a69"), Name = "superadmin" });
        }
    }
}
