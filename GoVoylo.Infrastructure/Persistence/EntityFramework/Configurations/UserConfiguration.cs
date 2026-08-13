using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("gv_users");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            // Email
            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(255);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            // Phone
            builder.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20);

            // Phone Country Code
            builder.Property(x => x.PhoneCountryCode)
                .HasColumnName("phone_country_code")
                .HasMaxLength(5);

            // Password
            builder.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255);

            // First Name
            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            // Last Name
            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            // Email Verified
            builder.Property(x => x.IsEmailVerified)
                .HasColumnName("is_email_verified")
                .HasDefaultValue(false)
                .IsRequired();

            // Phone Verified
            builder.Property(x => x.IsPhoneVerified)
                .HasColumnName("is_phone_verified")
                .HasDefaultValue(false)
                .IsRequired();

            // Profile Image
            builder.Property(x => x.ProfileImageUrl)
                .HasColumnName("profile_image_url")
                .HasMaxLength(500);

            // Status
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .IsRequired();

            // Created At
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Updated At
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            //// Email OR Phone must be present
            //builder.HasCheckConstraint(
            //    "chk_users_has_identifier",
            //    "email IS NOT NULL OR phone IS NOT NULL");

            builder.ToTable("gv_users", x =>
            {
                x.HasCheckConstraint(
                    "chk_users_has_identifier",
                    "email IS NOT NULL OR phone IS NOT NULL");
            });
        }
    }
}
