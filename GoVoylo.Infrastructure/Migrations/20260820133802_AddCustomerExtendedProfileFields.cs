using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerExtendedProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "anniversary",
                table: "gv_users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "auto_add_travel_insurance",
                table: "gv_users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "city_of_residence",
                table: "gv_users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_of_birth",
                table: "gv_users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "gv_users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marital_status",
                table: "gv_users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                table: "gv_users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "pan_card_number_encrypted",
                table: "gv_users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "passport_expiry_date",
                table: "gv_users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "passport_issuing_country",
                table: "gv_users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "passport_number_encrypted",
                table: "gv_users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "gv_users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "anniversary",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "auto_add_travel_insurance",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "city_of_residence",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "marital_status",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "nationality",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "pan_card_number_encrypted",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "passport_expiry_date",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "passport_issuing_country",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "passport_number_encrypted",
                table: "gv_users");

            migrationBuilder.DropColumn(
                name: "state",
                table: "gv_users");
        }
    }
}
