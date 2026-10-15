using FluentAssertions;
using GoVoylo.Application.Features.Flights;
using GoVoylo.Application.Features.Traveler.Commands.AddTraveler;

namespace GoVoylo.Application.UnitTests.Features.Flights
{
    public class BookingNamesTests
    {
        [Theory]
        [InlineData("Ravi", null, "Ravi")]
        [InlineData("Ravi", "", "Ravi")]
        [InlineData("Ravi", "  ", "Ravi")]
        [InlineData("Ravi Kumar", " Sharma ", "Sharma")]
        public void ASingleNameTraveller_IsBookedWithTheFirstNameAsSurname(string first, string? last, string expected)
        {
            BookingNames.Surname(first, last).Should().Be(expected);
        }

        [Fact]
        public void ATravellerCanBeSavedWithoutALastName()
        {
            var command = new AddTravelerCommand(
                Guid.NewGuid(), "adult", "Ravi", "", new DateTime(1990, 1, 1), "Male", "India", null, null, false);

            new AddTravelerCommandValidator().Validate(command).IsValid.Should().BeTrue();
        }
    }
}
