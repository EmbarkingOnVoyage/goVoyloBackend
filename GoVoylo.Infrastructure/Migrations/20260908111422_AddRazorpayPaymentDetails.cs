using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayPaymentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "BookingPayments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "BookingPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderOrderId",
                table: "BookingPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderPaymentId",
                table: "BookingPayments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RazorpayPaymentDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RazorpaySignature = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RazorpayPaymentDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RazorpayPaymentDetails");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "BookingPayments");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "BookingPayments");

            migrationBuilder.DropColumn(
                name: "ProviderOrderId",
                table: "BookingPayments");

            migrationBuilder.DropColumn(
                name: "ProviderPaymentId",
                table: "BookingPayments");
        }
    }
}
