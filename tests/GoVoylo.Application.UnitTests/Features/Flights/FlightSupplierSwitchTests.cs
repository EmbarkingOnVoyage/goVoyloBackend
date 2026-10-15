using FluentAssertions;
using GoVoylo.Application.Features.Flights;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.GetFareCalendar;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights
{
    public class FlightSupplierSwitchTests
    {
        private static IConfiguration Config(params (string Key, string Value)[] values) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value))
                .Build();

        private static IFlightSupplierClient Supplier(string code)
        {
            var client = Substitute.For<IFlightSupplierClient>();
            client.SupplierCode.Returns(code);
            client.SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new SupplierFlightSearchResultDto("key", Array.Empty<SupplierFlightOptionDto>()));
            return client;
        }

        [Fact]
        public void ASupplierWithNoSetting_IsOn()
        {
            FlightSupplierSwitch.IsEnabled(Config(), "tripjack").Should().BeTrue();
        }

        [Fact]
        public void ASupplierSetToFalse_IsOff()
        {
            FlightSupplierSwitch.IsEnabled(Config(("FlightSuppliers:flyshop:Enabled", "false")), "flyshop").Should().BeFalse();
        }

        [Fact]
        public async Task Search_SkipsASupplierThatIsOff()
        {
            var flyshop = Supplier("flyshop");
            var tripjack = Supplier("tripjack");
            var discountRates = Substitute.For<ICommissionDiscountRateRepository>();
            var handler = new SearchFlightsQueryHandler(
                new[] { flyshop, tripjack },
                Substitute.For<IFlightSearchSessionStore>(),
                Substitute.For<ISender>(),
                discountRates,
                Config(("FlightSuppliers:flyshop:Enabled", "false")));

            var request = new FlightSearchRequestDto(
                "OneWay", "Economy", new[] { new FlightSearchSegmentDto("DEL", "BOM", new DateTime(2026, 12, 8)) }, 1, 0, 0);
            await handler.Handle(new SearchFlightsQuery(request), CancellationToken.None);

            await flyshop.DidNotReceive().SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>());
            await tripjack.Received(1).SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FareCalendar_IsEmptyWhenFlyshopIsOff()
        {
            var resolver = Substitute.For<IFlightSupplierClientResolver>();
            var handler = new GetFareCalendarQueryHandler(resolver, Config(("FlightSuppliers:flyshop:Enabled", "false")));

            var result = await handler.Handle(new GetFareCalendarQuery("DEL", "BOM", 12, 2026), CancellationToken.None);

            result.Days.Should().BeEmpty();
            resolver.DidNotReceive().Resolve(Arg.Any<string>());
        }
    }
}
