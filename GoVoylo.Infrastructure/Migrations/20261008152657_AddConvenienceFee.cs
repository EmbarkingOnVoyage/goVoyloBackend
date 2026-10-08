using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConvenienceFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "convenience_fee",
                table: "gv_trip_bookings",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "gv_convenience_fee_pax_bands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    min_pax = table.Column<int>(type: "integer", nullable: false),
                    factor = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_convenience_fee_pax_bands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gv_convenience_fee_trip_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    trip_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    first_journey_percent = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    extra_journey_percent = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    max_fee_per_pax = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_convenience_fee_trip_rates", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "gv_convenience_fee_pax_bands",
                columns: new[] { "id", "created_at", "factor", "is_active", "min_pax", "updated_at" },
                values: new object[,]
                {
                    { new Guid("8a3d5b2e-7c1f-4e9a-b604-3c2e1f9d8a01"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), 1.00m, true, 1, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8a3d5b2e-7c1f-4e9a-b604-3c2e1f9d8a02"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), 0.85m, true, 3, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8a3d5b2e-7c1f-4e9a-b604-3c2e1f9d8a03"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), 0.75m, true, 6, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "gv_convenience_fee_trip_rates",
                columns: new[] { "id", "created_at", "extra_journey_percent", "first_journey_percent", "is_active", "max_fee_per_pax", "trip_type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c01"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), 1.00m, 1.00m, true, null, "OneWay", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c02"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), 0.80m, 0.80m, true, null, "RoundTrip", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c03"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), 0.75m, 1.00m, true, null, "MultiCity", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ux_convenience_fee_pax_bands_min_pax",
                table: "gv_convenience_fee_pax_bands",
                column: "min_pax",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_convenience_fee_trip_rates_trip_type",
                table: "gv_convenience_fee_trip_rates",
                column: "trip_type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_convenience_fee_pax_bands");

            migrationBuilder.DropTable(
                name: "gv_convenience_fee_trip_rates");

            migrationBuilder.DropColumn(
                name: "convenience_fee",
                table: "gv_trip_bookings");
        }
    }
}
