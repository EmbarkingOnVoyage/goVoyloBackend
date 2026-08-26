using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelerManagementDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gv_saved_travelers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    traveler_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    meal_preference = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    seat_preference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_saved_travelers", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_saved_travelers_gv_users_user_id",
                        column: x => x.user_id,
                        principalTable: "gv_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gv_traveler_emergency_contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    saved_traveler_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    phone_country_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_traveler_emergency_contacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_traveler_emergency_contacts_gv_saved_travelers_saved_tra~",
                        column: x => x.saved_traveler_id,
                        principalTable: "gv_saved_travelers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gv_traveler_frequent_flyers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    saved_traveler_id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    membership_number_encrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_traveler_frequent_flyers", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_traveler_frequent_flyers_gv_saved_travelers_saved_travel~",
                        column: x => x.saved_traveler_id,
                        principalTable: "gv_saved_travelers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gv_traveler_passports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    saved_traveler_id = table.Column<Guid>(type: "uuid", nullable: false),
                    passport_number_encrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    issuing_country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "date", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_traveler_passports", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_traveler_passports_gv_saved_travelers_saved_traveler_id",
                        column: x => x.saved_traveler_id,
                        principalTable: "gv_saved_travelers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gv_traveler_special_assistance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    saved_traveler_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ssr_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_traveler_special_assistance", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_traveler_special_assistance_gv_saved_travelers_saved_tra~",
                        column: x => x.saved_traveler_id,
                        principalTable: "gv_saved_travelers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gv_traveler_visas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    saved_traveler_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    visa_number_encrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    visa_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    issue_date = table.Column<DateTime>(type: "date", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "date", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gv_traveler_visas", x => x.id);
                    table.ForeignKey(
                        name: "FK_gv_traveler_visas_gv_saved_travelers_saved_traveler_id",
                        column: x => x.saved_traveler_id,
                        principalTable: "gv_saved_travelers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_saved_travelers_user",
                table: "gv_saved_travelers",
                column: "user_id",
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_gv_traveler_emergency_contacts_saved_traveler_id",
                table: "gv_traveler_emergency_contacts",
                column: "saved_traveler_id");

            migrationBuilder.CreateIndex(
                name: "ux_traveler_frequent_flyer_airline",
                table: "gv_traveler_frequent_flyers",
                columns: new[] { "saved_traveler_id", "airline_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_traveler_passport",
                table: "gv_traveler_passports",
                column: "saved_traveler_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_traveler_special_assistance_code",
                table: "gv_traveler_special_assistance",
                columns: new[] { "saved_traveler_id", "ssr_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_traveler_visa_country",
                table: "gv_traveler_visas",
                columns: new[] { "saved_traveler_id", "country" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gv_traveler_emergency_contacts");

            migrationBuilder.DropTable(
                name: "gv_traveler_frequent_flyers");

            migrationBuilder.DropTable(
                name: "gv_traveler_passports");

            migrationBuilder.DropTable(
                name: "gv_traveler_special_assistance");

            migrationBuilder.DropTable(
                name: "gv_traveler_visas");

            migrationBuilder.DropTable(
                name: "gv_saved_travelers");
        }
    }
}
