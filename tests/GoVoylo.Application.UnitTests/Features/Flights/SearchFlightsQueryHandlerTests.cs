using FluentAssertions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights
{
    public class SearchFlightsQueryHandlerTests
    {
        private readonly IFlightSupplierClient _supplier = Substitute.For<IFlightSupplierClient>();
        private readonly IFlightSearchSessionStore _sessionStore = Substitute.For<IFlightSearchSessionStore>();
        private readonly ICommissionDiscountRateRepository _discountRates = Substitute.For<ICommissionDiscountRateRepository>();
        private readonly List<FlightOfferSession> _saved = new();
        private readonly SearchFlightsQueryHandler _handler;

        public SearchFlightsQueryHandlerTests()
        {
            _supplier.SupplierCode.Returns("flyshop");
            _sessionStore.SaveAsync(Arg.Do<FlightOfferSession>(s => _saved.Add(s)), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());
            _discountRates.GetPercentAtAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(100m);
            _handler = new SearchFlightsQueryHandler(new[] { _supplier }, _sessionStore, Substitute.For<ISender>(), _discountRates);
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
    
        // A Flyshop-like fare for 2 adults: ₹7,297 each, ₹500 commission each.
        private static SupplierFlightOptionDto CommissionedOption() =>
            new("key", "fare", "6E", "IndiGo", true, true, new[] { Segment("DEL", "BOM", "2026-12-08T08:00") }, 14594m, "INR", 9,
                new[]
                {
                    new SupplierFareOptionDto("fare", true, 7297m, "INR", null, null, 14594m,
                        BookingBaseAmount: 11000m, BookingCommissionAmount: 1000m, AdultCommission: 500m)
                });

        private async Task<(FlightOfferDto Offer, FlightOfferSession Session)> PriceAsync(decimal percent)
        {
            _discountRates.GetPercentAtAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(percent);
            _supplier.SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new SupplierFlightSearchResultDto("search", new[] { CommissionedOption() }));
            var response = await _handler.Handle(new SearchFlightsQuery(Request("OneWay", ("DEL", "BOM"))), CancellationToken.None);
            return (response.Offers.Single(), _saved.Single());
        }

        [Fact]
        public async Task AFullDiscount_ShowsTheCommissionStruckOut_AndChargesTheSupplierPrice()
        {
            var (offer, session) = await PriceAsync(100m);
            var fare = offer.Fares.Single();

            fare.BookingListAmount.Should().Be(15594m);
            fare.BookingDiscountAmount.Should().Be(1000m);
            fare.BookingTotalAmount.Should().Be(14594m);
            fare.ListAmount.Should().Be(7797m);
            fare.TotalAmount.Should().Be(7297m);
            fare.BookingCommissionAmount.Should().Be(1000m);
            offer.ListAmount.Should().Be(15594m);
            offer.TotalAmount.Should().Be(14594m);
            session.Fares!.Single().BookingCommission.Should().Be(1000m);
            session.Fares!.Single().BookingDiscount.Should().Be(1000m);
        }

        [Fact]
        public async Task APartialDiscount_ChargesTheCommissionNotGivenBack()
        {
            var (offer, session) = await PriceAsync(40m);
            var fare = offer.Fares.Single();

            fare.BookingListAmount.Should().Be(15594m);
            fare.BookingDiscountAmount.Should().Be(400m);
            fare.BookingTotalAmount.Should().Be(15194m);
            fare.TotalAmount.Should().Be(7597m);
            offer.TotalAmount.Should().Be(15194m);
            session.Fares!.Single().BookingDiscount.Should().Be(400m);
        }

        [Fact]
        public async Task AFareWithoutCommission_IsUnchanged()
        {
            _discountRates.GetPercentAtAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(40m);
            var offer = await SearchOfferAsync(Option(Segment("DEL", "GOI", "2026-12-08T08:00")));

            offer.TotalAmount.Should().Be(1000m);
            offer.DiscountAmount.Should().Be(0m);
            offer.Fares.Single().BookingListAmount.Should().Be(1000m);
        }

        private async Task<FlightOfferDto> SearchOfferAsync(SupplierFlightOptionDto option)
        {
            _supplier.SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new SupplierFlightSearchResultDto("search", new[] { option }));
            var response = await _handler.Handle(new SearchFlightsQuery(Request("OneWay", ("DEL", "GOI"))), CancellationToken.None);
            return response.Offers.Single();
        }
    }
}
