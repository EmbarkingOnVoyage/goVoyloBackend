using FluentAssertions;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Commands.SocialLogin;
using GoVoylo.Application.Features.Authentication.Services;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Authentication
{
    public class SocialLoginCommandHandlerTests
    {
        private readonly ISocialTokenVerifier _verifier = Substitute.For<ISocialTokenVerifier>();
        private readonly IAuthIdentityRepository _identities = Substitute.For<IAuthIdentityRepository>();
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly IUserRoleRepository _userRoles = Substitute.For<IUserRoleRepository>();
        private readonly SocialLoginCommandHandler _handler;

        private readonly IAccessAuditService _access = Substitute.For<IAccessAuditService>();

        public SocialLoginCommandHandlerTests()
        {
            var travelers = Substitute.For<ISavedTravelerRepository>();
            var bookings = Substitute.For<ITripBookingRepository>();
            _users.GetGuestsByEmailAsync(Arg.Any<string>()).Returns(new List<User>());
            _userRoles.GetRoleNamesForUserAsync(Arg.Any<Guid>()).Returns(new List<string> { "customer" });
            var jwt = Substitute.For<IJwtTokenService>();
            jwt.GenerateToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("access");
            var refresh = Substitute.For<IRefreshTokenService>();
            refresh.GenerateRawToken().Returns("refresh");
            refresh.Hash(Arg.Any<string>()).Returns("hash");
            refresh.GetExpiryDate().Returns(DateTime.UtcNow.AddDays(30));

            _handler = new SocialLoginCommandHandler(
                _verifier,
                _identities,
                _users,
                jwt,
                refresh,
                Substitute.For<IRefreshTokenRepository>(),
                _userRoles,
                Substitute.For<IRoleRepository>(),
                Substitute.For<IAuditService>(),
                _access,
                new GuestAccountMerger(_users, travelers, bookings, NullLogger<GuestAccountMerger>.Instance));
        }

        private void Verified(string? email, string subject = "google-sub-1") =>
            _verifier.VerifyAsync("google", "id-token", Arg.Any<CancellationToken>())
                .Returns(new VerifiedSocialIdentity("google", subject, email, "Pankaj", "Tayade"));

        [Fact]
        public async Task AnAlreadyLinkedProviderId_SignsIntoThatAccount()
        {
            var account = new User("pankaj@example.com", "Pankaj", "Tayade");
            Verified("someone-else@example.com");
            _identities.GetAsync("google", "google-sub-1", Arg.Any<CancellationToken>())
                .Returns(new AuthIdentity(account.Id, "google", "google-sub-1"));
            _users.GetByIdAsync(account.Id).Returns(account);

            var result = await _handler.Handle(new SocialLoginCommand("google", "id-token"), CancellationToken.None);

            result.Id.Should().Be(account.Id);
            result.AccessToken.Should().Be("access");
            await _identities.DidNotReceive().AddAsync(Arg.Any<AuthIdentity>(), Arg.Any<CancellationToken>());
            _access.Received(1).LogSignIn(account.Id, "google");
        }

        [Fact]
        public async Task FirstSignIn_WithAnExistingAccountsEmail_LinksToIt()
        {
            var account = new User("pankaj@example.com", "Pankaj", "Tayade");
            Verified("Pankaj@Example.com");
            _users.GetByEmailAsync("pankaj@example.com").Returns(account);

            var result = await _handler.Handle(new SocialLoginCommand("google", "id-token"), CancellationToken.None);

            result.Id.Should().Be(account.Id);
            await _identities.Received(1).AddAsync(
                Arg.Is<AuthIdentity>(i => i.UserId == account.Id && i.Provider == "google" && i.ProviderSubject == "google-sub-1"),
                Arg.Any<CancellationToken>());
            await _users.DidNotReceive().SaveAsync(Arg.Any<User>());
        }

        [Fact]
        public async Task FirstSignIn_WithANewEmail_CreatesTheAccount()
        {
            Verified("new@example.com");

            var result = await _handler.Handle(new SocialLoginCommand("google", "id-token"), CancellationToken.None);

            await _users.Received(1).SaveAsync(Arg.Is<User>(u =>
                u.Email == "new@example.com" && u.FirstName == "Pankaj" && u.LastName == "Tayade" && u.Id == result.Id));
            await _identities.Received(1).AddAsync(Arg.Any<AuthIdentity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task NoEmailFromTheProvider_AndNoLinkedAccount_IsRefused()
        {
            Verified(null);

            var act = () => _handler.Handle(new SocialLoginCommand("google", "id-token"), CancellationToken.None);

            (await act.Should().ThrowAsync<BusinessRuleException>()).Which.Code.Should().Be("email_required");
        }
    }
}
