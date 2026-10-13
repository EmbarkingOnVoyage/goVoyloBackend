using FluentAssertions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
using GoVoylo.Application.Interfaces;
using MediatR;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights
{
    public class SearchFlightsQueryHandlerTests
    {
        private readonly IFlightSupplierClient _supplier = Substitute.For<IFlightSupplierClient>();
        private readonly IFlightSearchSessionStore _sessionStore = Substitute.For<IFlightSearchSessionStore>();
        private readonly List<FlightOfferSession> _saved = new();
        private readonly SearchFlightsQueryHandler _handler;

        public SearchFlightsQueryHandlerTests()
        {
            _supplier.SupplierCode.Returns("flyshop");
            _sessionStore.SaveAsync(Arg.Do<FlightOfferSession>(s => _saved.Add(s)), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());
            _handler = new SearchFlightsQueryHandler(new[] { _supplier }, _sessionStore, Substitute.For<ISender>());
        }

        private static SupplierFlightSegmentDto Segment(string origin, string destination, string departure, int tripIndex = 0) =>
            new(origin, destination, "6E", "IndiGo", "100", DateTime.Parse(departure), DateTime.Parse(departure).AddHours(2), "02:00", tripIndex);

        private static SupplierFlightOptionDto Option(params SupplierFlightSegmentDto[] segments) =>
            new("key", "fare", "6E", "IndiGo", true, true, segments, 1000m, "INR", 9,
                new[] { new SupplierFareOptionDto("fare", true, 1000m, "INR", null, null) });

        private async Task<FlightOfferSession> SearchAsync(FlightSearchRequestDto request, SupplierFlightOptionDto option)
        {
            _supplier.SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new SupplierFlightSearchResultDto("search", new[] { option }));
            await _handler.Handle(new SearchFlightsQuery(request), CancellationToken.None);
            return _saved.Single();
        }

        private static FlightSearchRequestDto Request(string tripType, params (string From, string To)[] legs) =>
            new(tripType, "Economy", legs.Select(l => new FlightSearchSegmentDto(l.From, l.To, new DateTime(2026, 12, 8))).ToList(), 1, 0, 0);

        [Fact]
        public async Task ACombinedOutAndBackFare_IsRecordedAgainstItsTurnaroundPoint()
        {
            var session = await SearchAsync(
                Request("MultiCity", ("DEL", "GOI"), ("GOI", "DEL")),
                Option(Segment("DEL", "BOM", "2026-12-08T08:00"), Segment("BOM", "GOI", "2026-12-08T11:35"), Segment("GOI", "DEL", "2026-12-11T08:10")));

            session.Origin.Should().Be("DEL");
            session.Destination.Should().Be("GOI");
            session.Trips.Should().BeNull();
        }

        [Fact]
        public async Task AnOrdinaryOffer_KeepsItsLastArrival()
        {
            var session = await SearchAsync(
                Request("OneWay", ("DEL", "GOI")),
                Option(Segment("DEL", "BOM", "2026-12-08T08:00"), Segment("BOM", "GOI", "2026-12-08T11:35")));

            session.Destination.Should().Be("GOI");
        }

        [Fact]
        public async Task AWholeTripOfferWithTripIndexes_IsStillSplitIntoTrips()
        {
            var session = await SearchAsync(
                Request("RoundTrip", ("DEL", "DXB"), ("DXB", "DEL")),
                Option(Segment("DEL", "DXB", "2026-12-08T08:00", 0), Segment("DXB", "DEL", "2026-12-11T08:10", 1)));

            session.Trips.Should().HaveCount(2);
            session.Destination.Should().Be("DEL");
        }
    }
}
