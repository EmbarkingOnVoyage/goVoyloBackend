using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingCancellationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancel_code",
                table: "gv_trip_bookings",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cancellation_type",
                table: "gv_trip_bookings",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancel_code",
                table: "gv_trip_bookings");

            migrationBuilder.DropColumn(
                name: "cancellation_type",
                table: "gv_trip_bookings");
        }
    }
}
