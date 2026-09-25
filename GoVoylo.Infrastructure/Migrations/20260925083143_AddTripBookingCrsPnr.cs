using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingCrsPnr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "crs_pnr",
                table: "gv_trip_bookings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crs_pnr",
                table: "gv_trip_bookings");
        }
    }
}
