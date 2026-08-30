using FluentAssertions;
using GoVoylo.Application.Features.Flights.Queries.GetPopularRoutes;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights.Queries.GetPopularRoutes
{
    public class GetPopularRoutesQueryHandlerTests
    {
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly GetPopularRoutesQueryHandler _handler;

        public GetPopularRoutesQueryHandlerTests()
        {
            _searchLogRepository = Substitute.For<ISearchLogRepository>();
            _handler = new GetPopularRoutesQueryHandler(_searchLogRepository);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedRoutes_OrderedByRepositoryResult()
        {
            var routes = new List<PopularRoute>
            {
                new("BOM", "DEL", 42),
                new("DEL", "BLR", 17),
            };

            _searchLogRepository.GetPopularRoutesAsync(20).Returns(routes);

            var result = await _handler.Handle(new GetPopularRoutesQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].Origin.Should().Be("BOM");
            result[0].SearchCount.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoSearchesLogged()
        {
            _searchLogRepository.GetPopularRoutesAsync(20).Returns(new List<PopularRoute>());

            var result = await _handler.Handle(new GetPopularRoutesQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
