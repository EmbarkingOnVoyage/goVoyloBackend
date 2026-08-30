using FluentAssertions;
using GoVoylo.Application.Features.Flights.Queries.GetSearchHistory;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights.Queries.GetSearchHistory
{
    public class GetSearchHistoryQueryHandlerTests
    {
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly GetSearchHistoryQueryHandler _handler;

        public GetSearchHistoryQueryHandlerTests()
        {
            _searchLogRepository = Substitute.For<ISearchLogRepository>();
            _handler = new GetSearchHistoryQueryHandler(_searchLogRepository);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedHistory_ForUser()
        {
            var userId = Guid.NewGuid();
            var logs = new List<SearchLog>
            {
                new(userId, "BOM", "DEL", new DateTime(2026, 10, 1), "OneWay", "Economy"),
                new(userId, "DEL", "BLR", new DateTime(2026, 11, 1), "OneWay", "Business"),
            };

            _searchLogRepository.GetHistoryAsync(userId, 20).Returns(logs);

            var result = await _handler.Handle(new GetSearchHistoryQuery(userId), CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].Origin.Should().Be("BOM");
            result[0].Destination.Should().Be("DEL");
            result[1].CabinClass.Should().Be("Business");
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoHistoryExists()
        {
            var userId = Guid.NewGuid();
            _searchLogRepository.GetHistoryAsync(userId, 20).Returns(new List<SearchLog>());

            var result = await _handler.Handle(new GetSearchHistoryQuery(userId), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
