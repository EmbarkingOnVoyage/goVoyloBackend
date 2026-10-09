using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedTravelerAccountHolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_account_holder",
                table: "gv_saved_travelers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ux_saved_travelers_account_holder",
                table: "gv_saved_travelers",
                column: "user_id",
                unique: true,
                filter: "is_account_holder AND NOT is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_saved_travelers_account_holder",
                table: "gv_saved_travelers");

            migrationBuilder.DropColumn(
                name: "is_account_holder",
                table: "gv_saved_travelers");
        }
    }
}
