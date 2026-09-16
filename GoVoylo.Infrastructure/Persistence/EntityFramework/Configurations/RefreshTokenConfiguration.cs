using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class RefreshTokenConfiguration
        : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("gv_refresh_tokens");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            // User Id
            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            // Token Hash
            builder.Property(x => x.TokenHash)
                .HasColumnName("token_hash")
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(x => x.TokenHash)
                .IsUnique()
                .HasDatabaseName("ux_refresh_tokens_token_hash");

            // Device Info
            builder.Property(x => x.DeviceInfo)
                .HasColumnName("device_info")
                .HasMaxLength(255);

            // Expires At
            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            // Revoked At
            builder.Property(x => x.RevokedAt)
                .HasColumnName("revoked_at")
                .HasColumnType("timestamptz");

            // Created At
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // User relationship
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
