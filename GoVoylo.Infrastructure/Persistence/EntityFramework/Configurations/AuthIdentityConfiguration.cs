using GoVoylo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework.Configurations
{
    public class AuthIdentityConfiguration
        : IEntityTypeConfiguration<AuthIdentity>
    {
        public void Configure(EntityTypeBuilder<AuthIdentity> builder)
        {
            builder.ToTable("gv_auth_identities");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            // User Id
            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            // Provider-google/apple/otp
            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasMaxLength(20)
                .IsRequired();

            // Provider Subject
            builder.Property(x => x.ProviderSubject)
                .HasColumnName("provider_subject")
                .HasMaxLength(255)
                .IsRequired();

            // Created At
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Unique (provider, provider_subject)
            builder.HasIndex(x => new
            {
                x.Provider,
                x.ProviderSubject
            })
            .IsUnique()
            .HasDatabaseName("ux_auth_identities_provider_subject");

            // User relationship
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
