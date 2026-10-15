// tests/GoVoylo.Domain.UnitTests/Entities/TripBookingTests.cs
using GoVoylo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace GoVoylo.Domain.UnitTests.Entities;

public class TripBookingTests
{
    private static TripBooking CreateDeferredBooking()
    {
        var booking = new TripBooking(
            Guid.NewGuid(), "tripjack", "TJS100000000001", null, null, null, "33",
            1588.50m, "INR", "Pankaj Tayade", "1");
        booking.SetDeferredSupplierPayload(new byte[] { 1, 2, 3 });
        return booking;
    }

    [Fact]
    public void MarkTicketed_ShouldClearDeferredPayload()
    {
        var booking = CreateDeferredBooking();

        booking.MarkTicketed("11", "ABC123", null, null);

        booking.StatusId.Should().Be("11");
        booking.DeferredSupplierPayloadEncrypted.Should().BeNull();
    }

    [Fact]
    public void MarkTicketingFailed_ShouldMarkFailedAndClearDeferredPayload()
    {
        var booking = CreateDeferredBooking();

        booking.MarkTicketingFailed();

        booking.StatusId.Should().Be("22");
        booking.DeferredSupplierPayloadEncrypted.Should().BeNull();
    }

    [Fact]
    public void PayableAmount_AddsTheConvenienceFee()
    {
        var booking = CreateDeferredBooking();

        booking.SetConvenienceFee(16m);

        booking.ConvenienceFee.Should().Be(16m);
        // 1,588.50 + 16 = 1,604.50, charged in whole rupees.
        booking.PayableAmount.Should().Be(1605m);
    }

    [Fact]
    public void SetConvenienceFee_RejectsNegative()
    {
        var booking = CreateDeferredBooking();

        var act = () => booking.SetConvenienceFee(-1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TotalPaid_UsesTheVerifiedCharge_WhichIncludesAppPricedAddOns()
    {
        var booking = CreateDeferredBooking();
        booking.SetConvenienceFee(16m);

        booking.TotalPaid.Should().Be(1605m);

        // Fare 1,588.50 + meals 800 + fee 16, charged through Razorpay in whole rupees.
        booking.RecordPayment(2405m);

        booking.AmountPaid.Should().Be(2405m);
        booking.TotalPaid.Should().Be(2405m);
        booking.PayableAmount.Should().Be(1605m);
    }

    [Fact]
    public void RecordPayment_RejectsNonPositive()
    {
        var booking = CreateDeferredBooking();

        var act = () => booking.RecordPayment(0m);

        act.Should().Throw<ArgumentException>();
    }
}
