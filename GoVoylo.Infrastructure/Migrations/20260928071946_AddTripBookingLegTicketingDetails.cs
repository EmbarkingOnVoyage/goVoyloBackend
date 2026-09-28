using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingLegTicketingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "airline_pnr",
                table: "gv_trip_booking_legs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crs_pnr",
                table: "gv_trip_booking_legs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "record_locator",
                table: "gv_trip_booking_legs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "airline_pnr",
                table: "gv_trip_booking_legs");

            migrationBuilder.DropColumn(
                name: "crs_pnr",
                table: "gv_trip_booking_legs");

            migrationBuilder.DropColumn(
                name: "record_locator",
                table: "gv_trip_booking_legs");
        }
    }
}
