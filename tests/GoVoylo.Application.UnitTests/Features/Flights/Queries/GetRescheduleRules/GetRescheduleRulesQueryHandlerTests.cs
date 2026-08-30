using FluentAssertions;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Flights.Queries.GetRescheduleRules;
using GoVoylo.Application.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights.Queries.GetRescheduleRules
{
    public class GetRescheduleRulesQueryHandlerTests
    {
        private readonly IFlightSearchResultCache _resultCache;
        private readonly GetRescheduleRulesQueryHandler _handler;

        public GetRescheduleRulesQueryHandlerTests()
        {
            _resultCache = Substitute.For<IFlightSearchResultCache>();
            _handler = new GetRescheduleRulesQueryHandler(_resultCache);
        }

        private static FlightOfferDto BuildOffer(Guid offerId, params RescheduleChargeDto[] charges) => new(
            offerId,
            "AI",
            "Air India",
            true,
            false,
            new List<FlightOfferSegmentDto>
            {
                new("BOM", "DEL", "AI", "101", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), "02:00"),
            },
            5000m,
            "INR",
            5,
            new FareBreakdownDto(4000m, 500m, new List<FareTaxDto>(), 0m, 0m, 0m, 0m, 0m, 5000m, "INR"),
            new BaggageDto("15 KG", null),
            charges.ToList());

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenSearchExpired()
        {
            var searchId = Guid.NewGuid();
            _resultCache.GetAsync(searchId, Arg.Any<CancellationToken>()).Returns((IReadOnlyList<FlightOfferDto>?)null);

            var act = () => _handler.Handle(
                new GetRescheduleRulesQuery(searchId, Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenOfferNotInSearchResult()
        {
            var searchId = Guid.NewGuid();
            var offer = BuildOffer(Guid.NewGuid());
            _resultCache.GetAsync(searchId, Arg.Any<CancellationToken>())
                .Returns(new List<FlightOfferDto> { offer });

            var act = () => _handler.Handle(
                new GetRescheduleRulesQuery(searchId, Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldReturnRescheduleCharges_ForMatchingOffer()
        {
            var searchId = Guid.NewGuid();
            var offerId = Guid.NewGuid();
            var charge = new RescheduleChargeDto(0, "2004.00", 0, 5, 100, 1, 1, 0m, 0m, "5000");
            var offer = BuildOffer(offerId, charge);

            _resultCache.GetAsync(searchId, Arg.Any<CancellationToken>())
                .Returns(new List<FlightOfferDto> { offer });

            var result = await _handler.Handle(
                new GetRescheduleRulesQuery(searchId, offerId), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Value.Should().Be("2004.00");
        }
    }
}
