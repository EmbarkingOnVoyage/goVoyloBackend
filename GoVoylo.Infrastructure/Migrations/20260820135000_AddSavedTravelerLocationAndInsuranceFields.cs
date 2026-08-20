using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedTravelerLocationAndInsuranceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "auto_add_travel_insurance",
                table: "gv_saved_travelers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "gv_saved_travelers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "gv_saved_travelers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auto_add_travel_insurance",
                table: "gv_saved_travelers");

            migrationBuilder.DropColumn(
                name: "city",
                table: "gv_saved_travelers");

            migrationBuilder.DropColumn(
                name: "state",
                table: "gv_saved_travelers");
        }
    }
}
