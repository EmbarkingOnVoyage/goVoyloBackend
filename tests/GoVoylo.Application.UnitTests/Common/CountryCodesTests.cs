using FluentAssertions;
using GoVoylo.Application.Common;

namespace GoVoylo.Application.UnitTests.Common
{
    public class CountryCodesTests
    {
        [Theory]
        [InlineData("India", "IN")]
        [InlineData("united arab emirates", "AE")]
        [InlineData("Czech Republic", "CZ")]
        [InlineData("South Korea", "KR")]
        [InlineData(" Thailand ", "TH")]
        public void ToIso2_ShouldMapPickerNames(string country, string expected)
        {
            CountryCodes.ToIso2(country).Should().Be(expected);
        }

        [Fact]
        public void ToIso2_ShouldPassThroughIsoCodes()
        {
            CountryCodes.ToIso2("in").Should().Be("IN");
        }

        [Fact]
        public void ToIso2_ShouldReturnUnknownNamesUnchanged()
        {
            CountryCodes.ToIso2("Atlantis").Should().Be("Atlantis");
        }
    }
}
