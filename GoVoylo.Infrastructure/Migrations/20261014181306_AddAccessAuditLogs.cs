using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gv_access_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sign_in_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    booking_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gateway_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gateway_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    payment_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    payment_method_detail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    isp = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    connection_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    is_vpn = table.Column<bool>(type: "boolean", nullable: true),
                    is_proxy = table.Column<bool>(type: "boolean", nullable: true),
                    is_tor = table.Column<bool>(type: "boolean", nullable: true),
                    is_hosting = table.Column<bool>(type: "boolean", nullable: true),
                    risk_score = table.Column<int>(type: "integer", nullable: true),
                    is_secure = table.Column<bool>(type: "boolean", nullable: true),
                    lookup_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_access_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_access_audit_logs_created_at",
                table: "gv_access_audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_access_audit_logs_gateway_order",
                table: "gv_access_audit_logs",
                column: "gateway_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_audit_logs_ip",
                table: "gv_access_audit_logs",
                columns: new[] { "ip_address", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_access_audit_logs_user",
                table: "gv_access_audit_logs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_access_audit_logs");
        }
    }
}
