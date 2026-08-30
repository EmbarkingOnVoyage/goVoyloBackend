using FluentAssertions;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.GetFareRules;
using GoVoylo.Application.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights.Queries.GetFareRules
{
    public class GetFareRulesQueryHandlerTests
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly GetFareRulesQueryHandler _handler;

        public GetFareRulesQueryHandlerTests()
        {
            _supplierClient = Substitute.For<IFlightSupplierClient>();
            _sessionStore = Substitute.For<IFlightSearchSessionStore>();
            _handler = new GetFareRulesQueryHandler(_supplierClient, _sessionStore);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenOfferSessionMissing()
        {
            var offerId = Guid.NewGuid();
            _sessionStore.GetAsync(offerId, Arg.Any<CancellationToken>()).Returns((FlightOfferSession?)null);

            var act = () => _handler.Handle(new GetFareRulesQuery(offerId), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldCallSupplierWithSessionKeys_AndMapRules()
        {
            var offerId = Guid.NewGuid();
            var session = new FlightOfferSession("TRIPJACK", "search-key", "flight-key", "fare-id");
            _sessionStore.GetAsync(offerId, Arg.Any<CancellationToken>()).Returns(session);

            _supplierClient
                .GetFareRulesAsync("search-key", "flight-key", "fare-id", Arg.Any<CancellationToken>())
                .Returns(new SupplierFareRulesResultDto(new List<SupplierFareRuleDto>
                {
                    new("0", "Universal", "<p>Cancellation allowed.</p>"),
                }));

            var result = await _handler.Handle(new GetFareRulesQuery(offerId), CancellationToken.None);

            result.OfferId.Should().Be(offerId);
            result.Rules.Should().ContainSingle();
            result.Rules[0].FareRuleName.Should().Be("Universal");
            result.Rules[0].FareRuleDescriptionHtml.Should().Contain("Cancellation allowed");
        }
    }
}
