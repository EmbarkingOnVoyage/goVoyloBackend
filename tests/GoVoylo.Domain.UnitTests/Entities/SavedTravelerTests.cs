// tests/GoVoylo.Domain.UnitTests/Entities/SavedTravelerTests.cs
using GoVoylo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace GoVoylo.Domain.UnitTests.Entities;

public class SavedTravelerTests
{
    private static SavedTraveler Create(string? email, string? phone, string? phoneCountryCode) =>
        new(Guid.NewGuid(), "adult", "Priya", "Sharma", new DateTime(1990, 4, 12), "Female", "IN",
            email: email, phone: phone, phoneCountryCode: phoneCountryCode);

    [Fact]
    public void Contact_IsStoredTrimmed()
    {
        var traveler = Create(" priya@example.com ", " 9876543210 ", "+91");

        traveler.Email.Should().Be("priya@example.com");
        traveler.Phone.Should().Be("9876543210");
        traveler.PhoneCountryCode.Should().Be("+91");
    }

    [Fact]
    public void Phone_WithoutCountryCode_DefaultsToIndia()
    {
        Create(null, "9876543210", null).PhoneCountryCode.Should().Be("+91");
    }

    [Fact]
    public void BlankContact_IsCleared()
    {
        var traveler = Create("", "  ", "+91");

        traveler.Email.Should().BeNull();
        traveler.Phone.Should().BeNull();
        traveler.PhoneCountryCode.Should().BeNull();
    }

    [Fact]
    public void Update_ReplacesContact()
    {
        var traveler = Create("old@example.com", "9876543210", "+91");

        traveler.Update("adult", "Priya", "Sharma", new DateTime(1990, 4, 12), "Female", "IN", null, null, false,
            "new@example.com", "4155550100", "+1");

        traveler.Email.Should().Be("new@example.com");
        traveler.Phone.Should().Be("4155550100");
        traveler.PhoneCountryCode.Should().Be("+1");
    }
}
