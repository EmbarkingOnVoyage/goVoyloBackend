using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class OtpConfiguration
        : IEntityTypeConfiguration<Otp>
    {
        public void Configure(EntityTypeBuilder<Otp> builder)
        {
            builder.ToTable("gv_otp");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            // User Id
            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            // Destination
            builder.Property(x => x.Destination)
                .HasColumnName("destination")
                .HasMaxLength(255)
                .IsRequired();

            // Purpose
            builder.Property(x => x.Purpose)
                .HasColumnName("purpose")
                .HasMaxLength(30)
                .IsRequired();

            // OTP Hash
            builder.Property(x => x.OtpHash)
                .HasColumnName("otp_hash")
                .HasMaxLength(255)
                .IsRequired();

            // Expires At
            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            // Consumed At
            builder.Property(x => x.ConsumedAt)
                .HasColumnName("consumed_at")
                .HasColumnType("timestamptz");

            // Attempt Count
            builder.Property(x => x.AttemptCount)
                .HasColumnName("attempt_count")
                .HasDefaultValue((short)0)
                .IsRequired();

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
