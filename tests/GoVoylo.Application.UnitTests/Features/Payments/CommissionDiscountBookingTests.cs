using FluentAssertions;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.UnitTests.Features.Payments
{
    public class CommissionDiscountBookingTests
    {
        private static TripBooking Booking(decimal supplierTotal) =>
            new(Guid.NewGuid(), "FLYSHOP", "REF1", "PNR1", null, null, "1", supplierTotal, "INR", "Pax One", "1");

        [Fact]
        public void AFullDiscount_LeavesThePayableAmountAtTheSupplierPrice()
        {
            var booking = Booking(7297m);
            booking.SetConvenienceFee(73m);
            booking.SetCommissionDiscount(500m, 500m);

            booking.PayableAmount.Should().Be(7370m);
        }

        [Fact]
        public void APartialDiscount_AddsTheCommissionNotGivenBack()
        {
            var booking = Booking(7297m);
            booking.SetConvenienceFee(73m);
            booking.SetCommissionDiscount(500m, 200m);

            booking.PayableAmount.Should().Be(7670m);
        }

        [Fact]
        public void ADiscountAboveTheCommission_IsRejected()
        {
            var act = () => Booking(7297m).SetCommissionDiscount(500m, 600m);
            act.Should().Throw<ArgumentException>();
        }
    }
}
