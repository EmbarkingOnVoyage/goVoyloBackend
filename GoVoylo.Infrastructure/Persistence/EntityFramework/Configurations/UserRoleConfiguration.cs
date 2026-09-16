using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("gv_user_roles");

            // Composite Primary Key
            builder.HasKey(x => new
            {
                x.UserId,
                x.RoleId
            });

            // User Id
            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            // Role Id
            builder.Property(x => x.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            // Granted At
            builder.Property(x => x.GrantedAt)
                .HasColumnName("granted_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // User relationship
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role relationship
            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
