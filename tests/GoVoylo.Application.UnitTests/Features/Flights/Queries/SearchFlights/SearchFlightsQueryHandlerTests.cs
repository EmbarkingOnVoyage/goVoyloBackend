using FluentAssertions;
using GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.SearchFlights;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryHandlerTests
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly IFlightSearchResultCache _resultCache;
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly ISender _mediator;
        private readonly SearchFlightsQueryHandler _handler;

        public SearchFlightsQueryHandlerTests()
        {
            _supplierClient = Substitute.For<IFlightSupplierClient>();
            _sessionStore = Substitute.For<IFlightSearchSessionStore>();
            _resultCache = Substitute.For<IFlightSearchResultCache>();
            _searchLogRepository = Substitute.For<ISearchLogRepository>();
            _mediator = Substitute.For<ISender>();

            _supplierClient.SupplierCode.Returns("TRIPJACK");
            _supplierClient
                .SearchAsync(Arg.Any<FlightSearchRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new SupplierFlightSearchResultDto("search-key", new List<SupplierFlightOptionDto>()));
            _sessionStore
                .SaveAsync(Arg.Any<FlightOfferSession>(), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());

            _handler = new SearchFlightsQueryHandler(
                _supplierClient, _sessionStore, _resultCache, _searchLogRepository, _mediator);
        }

        private static FlightSearchRequestDto BuildRequest() =>
            new(
                "OneWay",
                "Economy",
                new List<FlightSearchSegmentDto> { new("BOM", "DEL", new DateTime(2026, 10, 15)) },
                1,
                0,
                0);

        [Fact]
        public async Task Handle_ShouldLogSearch_EvenForGuestSearch()
        {
            await _handler.Handle(new SearchFlightsQuery(BuildRequest()), CancellationToken.None);

            await _searchLogRepository.Received(1).AddAsync(Arg.Is<SearchLog>(
                x => x.Origin == "BOM" && x.Destination == "DEL" && x.UserId == null));
        }

        [Fact]
        public async Task Handle_ShouldNotSaveRecentAirportSearch_WhenUserIdIsNull()
        {
            await _handler.Handle(new SearchFlightsQuery(BuildRequest()), CancellationToken.None);

            await _mediator.DidNotReceive().Send(
                Arg.Any<SaveRecentAirportSearchCommand>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldSaveRecentAirportSearchForEachLeg_WhenUserIdProvided()
        {
            var userId = Guid.NewGuid();

            await _handler.Handle(new SearchFlightsQuery(BuildRequest(), userId), CancellationToken.None);

            await _mediator.Received(1).Send(
                Arg.Is<SaveRecentAirportSearchCommand>(x => x.UserId == userId && x.IataCode == "BOM"),
                Arg.Any<CancellationToken>());
            await _mediator.Received(1).Send(
                Arg.Is<SaveRecentAirportSearchCommand>(x => x.UserId == userId && x.IataCode == "DEL"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldPopulateResultCache_AndReturnSearchId()
        {
            var result = await _handler.Handle(new SearchFlightsQuery(BuildRequest()), CancellationToken.None);

            result.SearchId.Should().NotBeEmpty();
            await _resultCache.Received(1).SaveAsync(
                result.SearchId, Arg.Any<IReadOnlyList<FlightOfferDto>>(), Arg.Any<CancellationToken>());
        }
    }
}
