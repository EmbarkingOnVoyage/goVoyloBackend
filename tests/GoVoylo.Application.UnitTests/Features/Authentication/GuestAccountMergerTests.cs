using FluentAssertions;
using GoVoylo.Application.Features.Authentication.Services;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Authentication
{
    public class GuestAccountMergerTests
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly ISavedTravelerRepository _travelerRepository = Substitute.For<ISavedTravelerRepository>();
        private readonly ITripBookingRepository _tripBookingRepository = Substitute.For<ITripBookingRepository>();
        private readonly GuestAccountMerger _merger;

        private readonly User _account = new("pankaj@example.com", "Pankaj", "Tayade");
        private readonly User _guest = User.CreateGuest("Pankaj@Example.com ", "9123456780");

        public GuestAccountMergerTests()
        {
            _merger = new GuestAccountMerger(
                _userRepository, _travelerRepository, _tripBookingRepository,
                NullLogger<GuestAccountMerger>.Instance);
            _userRepository.GetGuestsByEmailAsync("pankaj@example.com").Returns(new List<User> { _guest });
            _travelerRepository.GetByUserIdAsync(Arg.Any<Guid>()).Returns(new List<SavedTraveler>());
            _tripBookingRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(new List<TripBooking>());
        }

        private static SavedTraveler Traveler(Guid userId, string first, string last) =>
            new(userId, "adult", first, last, new DateTime(1990, 6, 15), "Male", "India");

        [Fact]
        public void AGuest_KeepsItsEmailOutOfTheSignInEmail()
        {
            _guest.Email.Should().BeNull();
            _guest.GuestEmail.Should().Be("pankaj@example.com");
            _guest.ContactEmail.Should().Be("pankaj@example.com");
            _guest.IsGuest.Should().BeTrue();

            _guest.SetContactDetails(null, "someone@example.com");
            _guest.Email.Should().BeNull();
        }

        [Fact]
        public async Task MovesTheGuestsTravellersAndBookings_IntoTheAccount()
        {
            var traveler = Traveler(_guest.Id, "Self", "Tester");
            _travelerRepository.GetByUserIdAsync(_guest.Id).Returns(new List<SavedTraveler> { traveler });
            var booking = new TripBooking(_guest.Id, "FLYSHOP", "REF1", null, null, null, "1", 1000m, "INR", "Self Tester", "1");
            _tripBookingRepository.GetByUserIdAsync(_guest.Id, Arg.Any<CancellationToken>())
                .Returns(new List<TripBooking> { booking });

            await _merger.MergeIntoAsync(_account, CancellationToken.None);

            traveler.UserId.Should().Be(_account.Id);
            booking.UserId.Should().Be(_account.Id);
            _guest.Status.Should().Be(User.MergedGuestStatus);
            _account.Phone.Should().Be("9123456780");
            await _tripBookingRepository.Received(1).UpdateAsync(booking, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DropsAGuestTraveller_TheAccountAlreadyHas()
        {
            var own = Traveler(_account.Id, "Self", "Tester");
            var duplicate = Traveler(_guest.Id, "self", "TESTER");
            _travelerRepository.GetByUserIdAsync(_account.Id).Returns(new List<SavedTraveler> { own });
            _travelerRepository.GetByUserIdAsync(_guest.Id).Returns(new List<SavedTraveler> { duplicate });

            await _merger.MergeIntoAsync(_account, CancellationToken.None);

            duplicate.IsDeleted.Should().BeTrue();
            duplicate.UserId.Should().Be(_guest.Id);
        }

        [Fact]
        public async Task DoesNothing_WithoutGuestCheckouts()
        {
            _userRepository.GetGuestsByEmailAsync("pankaj@example.com").Returns(new List<User>());

            await _merger.MergeIntoAsync(_account, CancellationToken.None);

            await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
        }
    }
}
