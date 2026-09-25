using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingLegCancellationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancel_code",
                table: "gv_trip_booking_legs",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cancellation_type",
                table: "gv_trip_booking_legs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "gv_trip_booking_legs",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                table: "gv_trip_booking_legs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancel_code",
                table: "gv_trip_booking_legs");

            migrationBuilder.DropColumn(
                name: "cancellation_type",
                table: "gv_trip_booking_legs");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "gv_trip_booking_legs");

            migrationBuilder.DropColumn(
                name: "is_cancelled",
                table: "gv_trip_booking_legs");
        }
    }
}
