// tests/GoVoylo.Domain.UnitTests/Pricing/ConvenienceFeeCalculatorTests.cs
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Pricing;
using FluentAssertions;
using Xunit;

namespace GoVoylo.Domain.UnitTests.Pricing;

public class ConvenienceFeeCalculatorTests
{
    // The seeded rules (see the ConvenienceFeeOnFarePlusAddOns migration).
    private static readonly ConvenienceFeeTripRate OneWay = new(TripTypes.OneWay, 2.64m, 2.64m, fullRateJourneys: 1);
    private static readonly ConvenienceFeeTripRate RoundTrip = new(TripTypes.RoundTrip, 2.64m, 2.64m, fullRateJourneys: 2);
    private static readonly ConvenienceFeeTripRate MultiCity =
        new(TripTypes.MultiCity, 2.64m, 1.98m, fullRateJourneys: 2, applyPaxBands: false);

    private static readonly ConvenienceFeePaxBand[] Bands =
    {
        new(1, 1.00m),
        new(3, 0.85m),
        new(6, 0.75m),
    };

    // Every passenger pays 10,000 (fare + add-ons) per journey, so 2.64% is 264 a
    // passenger — the "300" of the agreed examples.
    private static JourneyFare[] Journeys(int count, int pax) =>
        Enumerable.Range(0, count).Select(_ => new JourneyFare(10_000m * pax)).ToArray();

    private static decimal Fee(ConvenienceFeeTripRate rate, int journeys, int pax) =>
        ConvenienceFeeCalculator.Calculate(rate, Bands, Journeys(journeys, pax), pax, 0);

    [Theory]
    [InlineData(1, 264)]                 // 1 x 264
    [InlineData(2, 528)]                 // 2 x 264
    [InlineData(3, 673.20)]              // 0.85 x (3 x 264)
    [InlineData(5, 1122)]                // 0.85 x (5 x 264)
    [InlineData(7, 1386)]                // 0.75 x (7 x 264)
    public void OneWay_GroupFactorOnEveryPassenger(int pax, decimal expected)
    {
        Fee(OneWay, 1, pax).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, 1056)]                // 2 x (2 x 264)
    [InlineData(3, 1346.40)]             // 2 x (0.85 x (3 x 264))
    [InlineData(7, 2772)]                // 2 x (0.75 x (7 x 264))
    public void RoundTrip_IsBothJourneysAtTheFullRate(int pax, decimal expected)
    {
        Fee(RoundTrip, 2, pax).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, 2, 1056)]             // 2 x (2 x 264)
    [InlineData(3, 2, 1452)]             // 2 x (2 x 264) + 1 x (0.75 x (2 x 264))
    [InlineData(5, 3, 3366)]             // 2 x (3 x 264) + 3 x (0.75 x (3 x 264)) — no group factor
    [InlineData(7, 5, 7590)]             // 2 x (5 x 264) + 5 x (0.75 x (5 x 264))
    public void MultiCity_FirstTwoJourneysFull_ThenSeventyFivePercent_NoGroupFactor(int journeys, int pax, decimal expected)
    {
        Fee(MultiCity, journeys, pax).Should().Be(expected);
    }

    [Fact]
    public void InfantsDontCountTowardsTheGroup()
    {
        // 2 adults (+ an infant, not in the amount or the count) stay at x1.
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { new JourneyFare(20_000m) }, 2, 0)
            .Should().Be(528m);
    }

    [Fact]
    public void ChildrenCountTowardsTheGroup()
    {
        // 2 adults + 1 child = 3 passengers -> x0.85
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, new[] { new JourneyFare(30_000m) }, 2, 1)
            .Should().Be(673.20m);
    }

    [Fact]
    public void NoPassengers_NoFee()
    {
        ConvenienceFeeCalculator.Calculate(OneWay, Bands, Journeys(1, 1), 0, 0).Should().Be(0m);
    }

    [Fact]
    public void ACap_LimitsEachPassenger()
    {
        var capped = new ConvenienceFeeTripRate(TripTypes.OneWay, 2.64m, 2.64m, maxFeePerPax: 200m);
        ConvenienceFeeCalculator.Calculate(capped, Bands, Journeys(1, 2), 2, 0).Should().Be(400m);
    }
}
