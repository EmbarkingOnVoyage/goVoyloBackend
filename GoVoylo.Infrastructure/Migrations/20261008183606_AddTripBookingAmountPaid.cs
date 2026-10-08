using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoVoylo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingAmountPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_paid",
                table: "gv_trip_bookings",
                type: "numeric(12,2)",
                nullable: true);

            // Backfill from Razorpay payments already verified (largest, if retried).
            migrationBuilder.Sql(@"
                UPDATE gv_trip_bookings t
                SET amount_paid = p.amount
                FROM (
                    SELECT ""BookingReference"" AS booking_ref, MAX(""TotalAmount"") AS amount
                    FROM ""BookingPayments""
                    WHERE ""PaymentStatus"" = 'Succeeded'
                    GROUP BY ""BookingReference""
                ) p
                WHERE p.booking_ref = t.booking_ref_no;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_paid",
                table: "gv_trip_bookings");
        }
    }
}
