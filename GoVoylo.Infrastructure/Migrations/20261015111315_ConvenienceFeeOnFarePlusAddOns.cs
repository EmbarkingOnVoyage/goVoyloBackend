using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvenienceFeeOnFarePlusAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "apply_pax_bands",
                table: "gv_convenience_fee_trip_rates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "full_rate_journeys",
                table: "gv_convenience_fee_trip_rates",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "gv_convenience_fee_trip_rates",
                keyColumn: "id",
                keyValue: new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c01"),
                columns: new[] { "apply_pax_bands", "extra_journey_percent", "first_journey_percent", "full_rate_journeys" },
                values: new object[] { true, 2.64m, 2.64m, 1 });

            migrationBuilder.UpdateData(
                table: "gv_convenience_fee_trip_rates",
                keyColumn: "id",
                keyValue: new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c02"),
                columns: new[] { "apply_pax_bands", "extra_journey_percent", "first_journey_percent", "full_rate_journeys" },
                values: new object[] { true, 2.64m, 2.64m, 2 });

            migrationBuilder.UpdateData(
                table: "gv_convenience_fee_trip_rates",
                keyColumn: "id",
                keyValue: new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c03"),
                columns: new[] { "apply_pax_bands", "extra_journey_percent", "first_journey_percent", "full_rate_journeys" },
                values: new object[] { false, 1.98m, 2.64m, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "apply_pax_bands",
                table: "gv_convenience_fee_trip_rates");

            migrationBuilder.DropColumn(
                name: "full_rate_journeys",
                table: "gv_convenience_fee_trip_rates");

            migrationBuilder.UpdateData(
                table: "gv_convenience_fee_trip_rates",
                keyColumn: "id",
                keyValue: new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c01"),
                columns: new[] { "extra_journey_percent", "first_journey_percent" },
                values: new object[] { 1.00m, 1.00m });

            migrationBuilder.UpdateData(
                table: "gv_convenience_fee_trip_rates",
                keyColumn: "id",
                keyValue: new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c02"),
                columns: new[] { "extra_journey_percent", "first_journey_percent" },
                values: new object[] { 0.80m, 0.80m });

            migrationBuilder.UpdateData(
                table: "gv_convenience_fee_trip_rates",
                keyColumn: "id",
                keyValue: new Guid("6f0e8c1a-2b4d-4c7e-9a31-1d5f2e8b7c03"),
                columns: new[] { "extra_journey_percent", "first_journey_percent" },
                values: new object[] { 0.75m, 1.00m });
        }
    }
}
