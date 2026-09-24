using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gv_trip_bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_ref_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    airline_pnr = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    record_locator = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    status_id = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    local_status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    passenger_names = table.Column<string>(type: "text", nullable: false),
                    pax_ids = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_trip_bookings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gv_trip_booking_legs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    trip_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leg_index = table.Column<int>(type: "integer", nullable: false),
                    origin = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    destination = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    travel_date = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    airline_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    airline_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    flight_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    flight_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_trip_booking_legs", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_trip_booking_legs_gv_trip_bookings_trip_booking_id",
                        column: x => x.trip_booking_id,
                        principalTable: "gv_trip_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trip_booking_legs_trip_booking_id",
                table: "gv_trip_booking_legs",
                column: "trip_booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_bookings_user_id",
                table: "gv_trip_bookings",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_trip_booking_legs");

            migrationBuilder.DropTable(
                name: "gv_trip_bookings");
        }
    }
}
