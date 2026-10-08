// tests/GoVoylo.Domain.UnitTests/Pricing/ConvenienceFeeCalculatorTests.cs
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Pricing;
using FluentAssertions;
using Xunit;

namespace GoVoylo.Domain.UnitTests.Pricing;

public class ConvenienceFeeCalculatorTests
{
    // The seeded rules (see the AddConvenienceFee migration).
    private static readonly ConvenienceFeeTripRate OneWay = new(TripTypes.OneWay, 1.00m, 1.00m);
    private static readonly ConvenienceFeeTripRate RoundTrip = new(TripTypes.RoundTrip, 0.80m, 0.80m);
    private static readonly ConvenienceFeeTripRate MultiCity = new(TripTypes.MultiCity, 1.00m, 0.75m);

    private static readonly ConvenienceFeePaxBand[] Bands =
    {
        new(1, 1.00m),
        new(3, 0.85m),
        new(6, 0.75m),
    };

    private static JourneyBaseFare Journey(decimal adult, decimal? child = null) => new(adult, child ?? adult);

    [Fact]
    public void OneWay_OneAdult_IsOnePercentOfBaseFare()
    {
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { Journey(5000m) }, 1, 0)
            .Should().Be(50.00m);
    }

    [Fact]
    public void RoundTrip_IsPointEightPercentOfEachJourney()
    {
        // 0.8% x 5,000 + 0.8% x 4,000
        ConvenienceFeeCalculator.Calculate(RoundTrip, Bands, new[] { Journey(5000m), Journey(4000m) }, 1, 0)
            .Should().Be(72.00m);
    }

    [Fact]
    public void MultiCity_FirstJourneyOnePercent_ExtraJourneysPointSevenFive()
    {
        // 1% x 5,000 + 0.75% x 4,000 + 0.75% x 3,000
        ConvenienceFeeCalculator.Calculate(
                MultiCity, Bands, new[] { Journey(5000m), Journey(4000m), Journey(3000m) }, 1, 0)
            .Should().Be(102.50m);
    }

    [Theory]
    [InlineData(1, 50.00)]   // 1–2 pax: factor 1.00
    [InlineData(2, 100.00)]
    [InlineData(3, 127.50)]  // 3–5 pax: factor 0.85 -> 42.50 each
    [InlineData(5, 212.50)]
    [InlineData(6, 225.00)]  // 6+ pax: factor 0.75 -> 37.50 each
    [InlineData(9, 337.50)]
    public void PaxBand_TapersTheFeePerPassenger(int adults, double expected)
    {
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { Journey(5000m) }, adults, 0)
            .Should().Be((decimal)expected);
    }

    [Fact]
    public void Children_PayOnTheirOwnBaseFare_AndCountTowardsTheBand()
    {
        // 2 adults + 1 child = 3 chargeable pax -> factor 0.85:
        // 2 x (1% x 5,000 x 0.85) + 1 x (1% x 3,000 x 0.85)
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { Journey(5000m, 3000m) }, 2, 1)
            .Should().Be(110.50m);
    }

    [Fact]
    public void NoChargeablePassengers_IsFree()
    {
        // Infants are never passed in as chargeable passengers.
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { Journey(5000m) }, 0, 0)
            .Should().Be(0m);
    }

    [Fact]
    public void MaxFeePerPax_CapsEachPassenger()
    {
        var capped = new ConvenienceFeeTripRate(TripTypes.MultiCity, 1.00m, 0.75m, maxFeePerPax: 80m);

        // Uncapped 102.50 per adult -> 80 each.
        ConvenienceFeeCalculator.Calculate(
                capped, Bands, new[] { Journey(5000m), Journey(4000m), Journey(3000m) }, 2, 0)
            .Should().Be(160.00m);
    }

    [Fact]
    public void Result_IsRoundedToTwoDecimals()
    {
        // 1% x 6,247.37 = 62.4737
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { Journey(6247.37m) }, 1, 0)
            .Should().Be(62.47m);
    }
}
