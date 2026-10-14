using FluentAssertions;
using GoVoylo.Application.Features.Traveler.Commands.AddTraveler;
using GoVoylo.Application.Features.Traveler.Services;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Traveler
{
    public class AccountHolderTravelerServiceTests
    {
        private readonly ISavedTravelerRepository _travelerRepository = Substitute.For<ISavedTravelerRepository>();
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly AccountHolderTravelerService _service;
        private readonly List<SavedTraveler> _saved = new();

        public AccountHolderTravelerServiceTests()
        {
            _service = new AccountHolderTravelerService(_travelerRepository, _userRepository);
            _travelerRepository.GetByUserIdAsync(Arg.Any<Guid>()).Returns(_ => _saved.ToList());
            _travelerRepository.AddAsync(Arg.Do<SavedTraveler>(t => _saved.Add(t))).Returns(Task.CompletedTask);
        }

        private User UserWith(string firstName, string lastName, DateTime? dateOfBirth)
        {
            var user = new User("pankaj@example.com", firstName, lastName);
            user.UpdateExtendedProfile("Male", dateOfBirth, "India", null, null, null, null, null, null, null, null, false);
            _userRepository.GetByIdAsync(user.Id).Returns(user);
            return user;
        }

        [Fact]
        public async Task CreatesTheAccountHolder_FromACompleteProfile_WithTheAccountEmail()
        {
            var user = UserWith("Pankaj", "Tayade", new DateTime(1987, 3, 3));

            var travelers = await _service.GetTravelersIncludingAccountHolderAsync(user.Id);

            var self = travelers.Should().ContainSingle().Subject;
            self.IsAccountHolder.Should().BeTrue();
            self.FirstName.Should().Be("Pankaj");
            self.TravelerType.Should().Be("adult");
            self.Email.Should().Be("pankaj@example.com");
        }

        [Fact]
        public async Task ReusesAMatchingCoTraveller_InsteadOfAddingADuplicate()
        {
            var user = UserWith("Pankaj", "Tayade", new DateTime(1987, 3, 3));
            var existing = new SavedTraveler(user.Id, "adult", "pankaj", "TAYADE", new DateTime(1987, 3, 3), "Male", "India");
            _saved.Add(existing);

            var travelers = await _service.GetTravelersIncludingAccountHolderAsync(user.Id);

            travelers.Should().ContainSingle();
            existing.IsAccountHolder.Should().BeTrue();
            existing.Email.Should().Be("pankaj@example.com");
            await _travelerRepository.DidNotReceive().AddAsync(Arg.Any<SavedTraveler>());
        }

        [Fact]
        public async Task SkipsAnIncompleteProfile()
        {
            // Accounts created by email OTP start with no last name or date of birth.
            var user = UserWith("pankaj", "", null);

            var travelers = await _service.GetTravelersIncludingAccountHolderAsync(user.Id);

            travelers.Should().BeEmpty();
            await _travelerRepository.DidNotReceive().AddAsync(Arg.Any<SavedTraveler>());
        }

        [Fact]
        public async Task LeavesAnExistingAccountHolderAlone()
        {
            var user = UserWith("Pankaj", "Tayade", new DateTime(1987, 3, 3));
            var self = new SavedTraveler(user.Id, "adult", "Pankaj", "Tayade", new DateTime(1987, 3, 3), "Male", "India");
            self.MarkAsAccountHolder("pankaj@example.com");
            _saved.Add(self);

            await _service.GetTravelersIncludingAccountHolderAsync(user.Id);

            await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
            await _travelerRepository.DidNotReceive().AddAsync(Arg.Any<SavedTraveler>());
        }
    
        [Fact]
        public async Task AddingYourself_UpdatesTheProfile()
        {
            // A new Google account: names only, no date of birth.
            var user = new User("pankaj@example.com", "pankaj", "");
            _userRepository.GetByIdAsync(user.Id).Returns(user);
            var handler = new AddTravelerCommandHandler(_travelerRepository, _service);

            await handler.Handle(new AddTravelerCommand(
                user.Id, "adult", "Pankaj", "Tayade", new DateTime(1987, 3, 3), "Male", "India", "Pune", "Maharashtra",
                false, "other@example.com", "9123456781", "+91", IsAccountHolder: true), CancellationToken.None);

            user.FirstName.Should().Be("Pankaj");
            user.LastName.Should().Be("Tayade");
            user.Gender.Should().Be("Male");
            user.DateOfBirth.Should().Be(new DateTime(1987, 3, 3));
            user.Nationality.Should().Be("India");
            user.CityOfResidence.Should().Be("Pune");
            user.State.Should().Be("Maharashtra");
            user.Phone.Should().Be("9123456781");
            user.Email.Should().Be("pankaj@example.com");
            await _userRepository.Received(1).UpdateAsync(user);
            _saved.Single().Email.Should().Be("pankaj@example.com");
        }

        [Fact]
        public async Task AddingACoTraveller_LeavesTheProfileAlone()
        {
            var user = UserWith("Pankaj", "Tayade", null);
            var handler = new AddTravelerCommandHandler(_travelerRepository, _service);

            await handler.Handle(new AddTravelerCommand(
                user.Id, "adult", "Riya", "Tayade", new DateTime(1990, 5, 5), "Female", "India", null, null, false),
                CancellationToken.None);

            user.FirstName.Should().Be("Pankaj");
            user.DateOfBirth.Should().BeNull();
            await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
        }
}
}
