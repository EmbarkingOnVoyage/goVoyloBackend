using FluentAssertions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Filtering;

namespace GoVoylo.Application.UnitTests.Features.Flights.Filtering
{
    public class FlightOfferFilterEngineTests
    {
        private static FlightOfferSegmentDto Segment(
            string origin, string destination, string airline, DateTime departure, DateTime arrival) =>
            new(origin, destination, airline, "101", departure, arrival, "02:00");

        private static readonly FareBreakdownDto EmptyFareBreakdown =
            new(0m, 0m, new List<FareTaxDto>(), 0m, 0m, 0m, 0m, 0m, 0m, "INR");

        private static readonly BaggageDto EmptyBaggage = new(null, null);

        private static FlightOfferDto NonStopOffer(
            string airline, decimal amount, DateTime departure, DateTime arrival, bool refundable = true) =>
            new(
                Guid.NewGuid(),
                airline,
                airline + " Airlines",
                refundable,
                false,
                new List<FlightOfferSegmentDto> { Segment("BOM", "DEL", airline, departure, arrival) },
                amount,
                "INR",
                5,
                EmptyFareBreakdown,
                EmptyBaggage,
                new List<RescheduleChargeDto>());

        private static FlightOfferDto OneStopOffer(
            string airline, decimal amount, DateTime departure, DateTime arrival) =>
            new(
                Guid.NewGuid(),
                airline,
                airline + " Airlines",
                true,
                false,
                new List<FlightOfferSegmentDto>
                {
                    Segment("BOM", "HYD", airline, departure, departure.AddHours(1)),
                    Segment("HYD", "DEL", airline, departure.AddHours(2), arrival),
                },
                amount,
                "INR",
                5,
                EmptyFareBreakdown,
                EmptyBaggage,
                new List<RescheduleChargeDto>());

        private static List<FlightOfferDto> SampleOffers() =>
            new()
            {
                NonStopOffer("AI", 5000m, new DateTime(2026, 10, 1, 6, 0, 0), new DateTime(2026, 10, 1, 8, 0, 0)),
                NonStopOffer("6E", 4000m, new DateTime(2026, 10, 1, 14, 0, 0), new DateTime(2026, 10, 1, 16, 15, 0), refundable: false),
                OneStopOffer("AI", 7000m, new DateTime(2026, 10, 1, 9, 0, 0), new DateTime(2026, 10, 1, 15, 0, 0)),
            };

        [Fact]
        public void Apply_ShouldFilterByPriceRange()
        {
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), 4500m, 6000m, null, null, null, null, null, null, null, null, null));

            result.Should().ContainSingle();
            result[0].TotalAmount.Should().Be(5000m);
        }

        [Fact]
        public void Apply_ShouldFilterByAirline()
        {
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), null, null, new[] { "6E" }, null, null, null, null, null, null, null, null));

            result.Should().ContainSingle();
            result[0].AirlineCode.Should().Be("6E");
        }

        [Fact]
        public void Apply_ShouldFilterByStopCount()
        {
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), null, null, null, new[] { 1 }, null, null, null, null, null, null, null));

            result.Should().ContainSingle();
            FlightOfferFilterEngine.GetStopCount(result[0]).Should().Be(1);
        }

        [Fact]
        public void Apply_ShouldFilterByRefundableOnly()
        {
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), null, null, null, null, null, null, null, null, null, true, null));

            result.Should().OnlyContain(o => o.Refundable);
            result.Should().HaveCount(2);
        }

        [Fact]
        public void Apply_ShouldFilterByMaxDuration()
        {
            // Non-stop AI (2h) and 6E (2h15m) qualify; the 1-stop AI (6h) does not.
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), null, null, null, null, null, null, null, null, null, null, null) with { MaxDurationMinutes = 180 });

            result.Should().HaveCount(2);
            result.Should().OnlyContain(o => FlightOfferFilterEngine.GetDurationMinutes(o) <= 180);
        }

        [Theory]
        [InlineData("price_asc", 4000)]
        [InlineData("price_desc", 7000)]
        public void Apply_ShouldSortByPrice(string sortBy, decimal expectedFirst)
        {
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), null, null, null, null, null, null, null, null, null, null, sortBy));

            result[0].TotalAmount.Should().Be(expectedFirst);
        }

        [Fact]
        public void Apply_ShouldSortByDurationAscending()
        {
            var result = FlightOfferFilterEngine.Apply(
                SampleOffers(),
                new FlightOfferFilterRequestDto(Guid.NewGuid(), null, null, null, null, null, null, null, null, null, null, "duration_asc"));

            FlightOfferFilterEngine.GetDurationMinutes(result[0]).Should().BeLessThanOrEqualTo(
                FlightOfferFilterEngine.GetDurationMinutes(result[^1]));
        }

        [Fact]
        public void GetStopCount_ShouldReturnZero_ForNonStopOffer()
        {
            var offer = NonStopOffer("AI", 5000m, DateTime.UtcNow, DateTime.UtcNow.AddHours(2));
            FlightOfferFilterEngine.GetStopCount(offer).Should().Be(0);
        }

        [Fact]
        public void GetStopCount_ShouldReturnOne_ForOneStopOffer()
        {
            var offer = OneStopOffer("AI", 7000m, DateTime.UtcNow, DateTime.UtcNow.AddHours(6));
            FlightOfferFilterEngine.GetStopCount(offer).Should().Be(1);
        }

        [Fact]
        public void Summarize_ShouldComputeMinMaxPriceAndDistinctAirlines()
        {
            var summary = FlightOfferFilterEngine.Summarize(SampleOffers());

            summary.MinPrice.Should().Be(4000m);
            summary.MaxPrice.Should().Be(7000m);
            summary.AirlineCodes.Should().BeEquivalentTo(new[] { "AI", "6E" });
            summary.StopCounts.Should().BeEquivalentTo(new[] { 0, 1 });
            summary.OfferCount.Should().Be(3);
        }

        [Fact]
        public void Summarize_ShouldReturnZeroedResult_ForEmptyList()
        {
            var summary = FlightOfferFilterEngine.Summarize(new List<FlightOfferDto>());

            summary.OfferCount.Should().Be(0);
            summary.MinPrice.Should().Be(0);
            summary.AirlineCodes.Should().BeEmpty();
        }
    }
}
