using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "commission_amount",
                table: "gv_trip_bookings",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "gv_trip_bookings",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "gv_commission_discount_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    percent = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_commission_discount_rates", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "gv_commission_discount_rates",
                columns: new[] { "id", "created_at", "effective_from", "note", "percent" },
                values: new object[] { new Guid("3c1d7b2e-5f4a-4e8b-9c62-0a7d1e4b9f01"), new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Launch offer: 100% of commission as discount (until at least 15 Nov 2026)", 100.000m });

            migrationBuilder.CreateIndex(
                name: "ix_commission_discount_rates_effective_from",
                table: "gv_commission_discount_rates",
                column: "effective_from");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_commission_discount_rates");

            migrationBuilder.DropColumn(
                name: "commission_amount",
                table: "gv_trip_bookings");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "gv_trip_bookings");
        }
    }
}
