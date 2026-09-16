using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleBasedAccessControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "gv_roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("4bbebc58-b75d-434d-bb5a-29c6bf7c8fe7"), "customer" },
                    { new Guid("9ebf67c2-40b6-4481-b580-895303558a69"), "superadmin" },
                    { new Guid("a04f3f16-299b-4087-a858-d12ca890794b"), "support_agent" }
                });

            // Backfill "customer" for every user that registered before this role
            // system existed — new registrations get it automatically going forward.
            migrationBuilder.Sql(@"
                INSERT INTO gv_user_roles (user_id, role_id, granted_at)
                SELECT u.id, '4bbebc58-b75d-434d-bb5a-29c6bf7c8fe7', now()
                FROM gv_users u
                WHERE NOT EXISTS (
                    SELECT 1 FROM gv_user_roles ur
                    WHERE ur.user_id = u.id AND ur.role_id = '4bbebc58-b75d-434d-bb5a-29c6bf7c8fe7'
                );
            ");

            // Seed the first superadmin by email — bootstraps role management without
            // a chicken-and-egg admin-only endpoint. No-op if that account doesn't exist yet.
            migrationBuilder.Sql(@"
                INSERT INTO gv_user_roles (user_id, role_id, granted_at)
                SELECT u.id, '9ebf67c2-40b6-4481-b580-895303558a69', now()
                FROM gv_users u
                WHERE u.email = 'pankaj.tayade@embarkingonvoyage.com'
                AND NOT EXISTS (
                    SELECT 1 FROM gv_user_roles ur
                    WHERE ur.user_id = u.id AND ur.role_id = '9ebf67c2-40b6-4481-b580-895303558a69'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "gv_roles",
                keyColumn: "id",
                keyValue: new Guid("4bbebc58-b75d-434d-bb5a-29c6bf7c8fe7"));

            migrationBuilder.DeleteData(
                table: "gv_roles",
                keyColumn: "id",
                keyValue: new Guid("9ebf67c2-40b6-4481-b580-895303558a69"));

            migrationBuilder.DeleteData(
                table: "gv_roles",
                keyColumn: "id",
                keyValue: new Guid("a04f3f16-299b-4087-a858-d12ca890794b"));
        }
    }
}
