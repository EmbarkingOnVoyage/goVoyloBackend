// tests/GoVoylo.Domain.UnitTests/Entities/UserContactDetailsTests.cs
using GoVoylo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace GoVoylo.Domain.UnitTests.Entities;

public class UserContactDetailsTests
{
    [Fact]
    public void SetContactDetails_SetsTheMobile_AndKeepsTheSignInEmail()
    {
        var user = new User("pankaj@example.com", "Pankaj", "");

        user.SetContactDetails("9876543210", "someone.else@example.com");

        user.Phone.Should().Be("9876543210");
        user.Email.Should().Be("pankaj@example.com");
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public void SetContactDetails_FillsInAMissingEmail_Unverified()
    {
        var user = new User(null!, "hash", null, "Pankaj", "Tayade");

        user.SetContactDetails("9876543210", " pankaj@example.com ");

        user.Email.Should().Be("pankaj@example.com");
        user.IsEmailVerified.Should().BeFalse();
    }
}
