using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gv_search_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origin = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    destination = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    travel_date = table.Column<DateTime>(type: "date", nullable: false),
                    trip_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cabin_class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    searched_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_search_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_search_logs_gv_users_user_id",
                        column: x => x.user_id,
                        principalTable: "gv_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_search_logs_route",
                table: "gv_search_logs",
                columns: new[] { "origin", "destination" });

            migrationBuilder.CreateIndex(
                name: "ix_search_logs_user_recency",
                table: "gv_search_logs",
                columns: new[] { "user_id", "searched_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_search_logs");
        }
    }
}
