using FluentAssertions;
using GoVoylo.Application.Features.Pricing.Services;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Payments
{
    public class ConvenienceFeeOfferTests
    {
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly ITripBookingRepository _bookings = Substitute.For<ITripBookingRepository>();

        private ConvenienceFeeService Service(string? until) => new(
            Substitute.For<IConvenienceFeeRuleRepository>(),
            _users,
            _bookings,
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ConvenienceFee:FirstBookingFreeUntil"] = until })
                .Build());

        private static readonly string NextYear = DateTimeOffset.UtcNow.AddYears(1).ToString("O");

        private User Account(params TripBooking[] bookings)
        {
            var user = new User("pankaj@example.com", "Pankaj", "Tayade");
            _users.GetByIdAsync(user.Id).Returns(user);
            _bookings.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(bookings);
            return user;
        }

        private static TripBooking Booking(bool paid)
        {
            var booking = new TripBooking(Guid.NewGuid(), "FLYSHOP", "REF1", "PNR1", null, null, "1", 6247m, "INR", "Pax One", "1");
            if (paid) booking.RecordPayment(6247m);
            return booking;
        }

        [Fact]
        public async Task AnAccountWithNoPaidBooking_GetsItsFirstBookingFree()
        {
            var user = Account();
            (await Service(NextYear).GetOfferAsync(user.Id, default)).FirstBookingFree.Should().BeTrue();
        }

        [Fact]
        public async Task AnUnpaidHold_DoesNotUseUpTheOffer()
        {
            var user = Account(Booking(paid: false));
            (await Service(NextYear).GetOfferAsync(user.Id, default)).FirstBookingFree.Should().BeTrue();
        }

        [Fact]
        public async Task APaidBooking_UsesUpTheOffer()
        {
            var user = Account(Booking(paid: false), Booking(paid: true));
            (await Service(NextYear).GetOfferAsync(user.Id, default)).FirstBookingFree.Should().BeFalse();
        }

        [Fact]
        public async Task GuestsNeverGetTheOffer()
        {
            var guest = User.CreateGuest("guest@example.com", "9123456780");
            _users.GetByIdAsync(guest.Id).Returns(guest);
            (await Service(NextYear).GetOfferAsync(guest.Id, default)).FirstBookingFree.Should().BeFalse();
        }

        [Theory]
        [InlineData("2020-11-15T23:59:59+05:30")]
        [InlineData(null)]
        [InlineData("not a date")]
        public async Task AfterTheEndDate_OrWithoutOne_ThereIsNoOffer(string? until)
        {
            var user = Account();
            (await Service(until).GetOfferAsync(user.Id, default)).FirstBookingFree.Should().BeFalse();
        }
    }
}
