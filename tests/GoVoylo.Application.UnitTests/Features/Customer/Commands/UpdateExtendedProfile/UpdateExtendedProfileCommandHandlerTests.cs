using System.Text;
using FluentAssertions;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Commands.UpdateExtendedProfile;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Customer.Commands.UpdateExtendedProfile
{
    public class UpdateExtendedProfileCommandHandlerTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly IEncryptionService _encryptionService;
        private readonly UpdateExtendedProfileCommandHandler _handler;

        public UpdateExtendedProfileCommandHandlerTests()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _auditService = Substitute.For<IAuditService>();
            _encryptionService = Substitute.For<IEncryptionService>();
            _encryptionService.Encrypt(Arg.Any<string>())
                .Returns(callInfo => Encoding.UTF8.GetBytes(callInfo.Arg<string>()));
            _encryptionService.Decrypt(Arg.Any<byte[]>())
                .Returns(callInfo => Encoding.UTF8.GetString(callInfo.Arg<byte[]>()));

            _handler = new UpdateExtendedProfileCommandHandler(_userRepository, _auditService, _encryptionService);
        }

        private static User BuildUserWithExistingPassportAndPan()
        {
            var user = new User("test@example.com", "hash", null, "Test", "User");
            user.UpdateExtendedProfile(
                "Male", new DateTime(1990, 1, 1), "Indian", "Single", null, "Pune", "Maharashtra",
                Encoding.UTF8.GetBytes("A1234567"), new DateTime(2030, 1, 1), "India",
                Encoding.UTF8.GetBytes("ABCDE1234F"), false);
            return user;
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _userRepository.GetByIdAsync(userId).Returns((User?)null);

            var command = new UpdateExtendedProfileCommand(
                userId, null, null, null, null, null, null, null, null, null, null, null, false);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldPreserveExistingPassportAndPan_WhenRequestLeavesThemEmpty()
        {
            // Guards against a real bug: the client only ever sees a masked passport/PAN
            // (never the real value), so it can't "resend" an unchanged one. Saving other
            // fields (e.g. just City) must not wipe the stored passport/PAN as a side effect.
            var user = BuildUserWithExistingPassportAndPan();
            _userRepository.GetByIdAsync(user.Id).Returns(user);

            var command = new UpdateExtendedProfileCommand(
                user.Id,
                Gender: "Male",
                DateOfBirth: new DateTime(1990, 1, 1),
                Nationality: "Indian",
                MaritalStatus: "Single",
                Anniversary: null,
                CityOfResidence: "Mumbai", // the only real change
                State: "Maharashtra",
                PassportNumber: null,
                PassportExpiryDate: null,
                PassportIssuingCountry: null,
                PanCardNumber: null,
                AutoAddTravelInsurance: false);

            await _handler.Handle(command, CancellationToken.None);

            user.CityOfResidence.Should().Be("Mumbai");
            user.PassportNumberEncrypted.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("A1234567"));
            user.PassportExpiryDate.Should().Be(new DateTime(2030, 1, 1));
            user.PassportIssuingCountry.Should().Be("India");
            user.PanCardNumberEncrypted.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("ABCDE1234F"));
        }

        [Fact]
        public async Task Handle_ShouldReplacePassportAndPan_WhenRequestProvidesNewValues()
        {
            var user = BuildUserWithExistingPassportAndPan();
            _userRepository.GetByIdAsync(user.Id).Returns(user);

            var command = new UpdateExtendedProfileCommand(
                user.Id,
                Gender: "Male",
                DateOfBirth: new DateTime(1990, 1, 1),
                Nationality: "Indian",
                MaritalStatus: "Single",
                Anniversary: null,
                CityOfResidence: "Pune",
                State: "Maharashtra",
                PassportNumber: "Z9999999",
                PassportExpiryDate: new DateTime(2032, 6, 1),
                PassportIssuingCountry: "USA",
                PanCardNumber: "ZYXWV9876G",
                AutoAddTravelInsurance: true);

            await _handler.Handle(command, CancellationToken.None);

            user.PassportNumberEncrypted.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("Z9999999"));
            user.PassportExpiryDate.Should().Be(new DateTime(2032, 6, 1));
            user.PassportIssuingCountry.Should().Be("USA");
            user.PanCardNumberEncrypted.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("ZYXWV9876G"));
            user.AutoAddTravelInsurance.Should().BeTrue();
        }
    }
}
