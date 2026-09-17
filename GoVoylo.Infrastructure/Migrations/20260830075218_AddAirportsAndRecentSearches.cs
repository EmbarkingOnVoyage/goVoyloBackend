using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAirportsAndRecentSearches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gv_airports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    iata_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_popular = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_airports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gv_recent_airport_searches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iata_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    searched_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_recent_airport_searches", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_recent_airport_searches_gv_users_user_id",
                        column: x => x.user_id,
                        principalTable: "gv_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_airports_city",
                table: "gv_airports",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "ux_airports_iata_code",
                table: "gv_airports",
                column: "iata_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recent_airport_searches_user_recency",
                table: "gv_recent_airport_searches",
                columns: new[] { "user_id", "searched_at" });

            migrationBuilder.CreateIndex(
                name: "ux_recent_airport_searches_user_iata",
                table: "gv_recent_airport_searches",
                columns: new[] { "user_id", "iata_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_airports");

            migrationBuilder.DropTable(
                name: "gv_recent_airport_searches");
        }
    }
}
