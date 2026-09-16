using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPassportExpiryAlertTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "passport_expiry_alert_sent_at",
                table: "gv_users",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_expiry_alert_sent_at",
                table: "gv_traveler_passports",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "passport_expiry_alert_sent_at",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "last_expiry_alert_sent_at",
                table: "gv_traveler_passports");
        }
    }
}
