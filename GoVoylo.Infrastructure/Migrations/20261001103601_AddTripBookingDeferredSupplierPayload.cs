using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingDeferredSupplierPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "deferred_supplier_payload_encrypted",
                table: "gv_trip_bookings",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deferred_supplier_payload_encrypted",
                table: "gv_trip_bookings");
        }
    }
}
