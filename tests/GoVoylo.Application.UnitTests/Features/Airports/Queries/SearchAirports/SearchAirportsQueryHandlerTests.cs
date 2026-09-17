using FluentAssertions;
using GoVoylo.Application.Features.Airports.Queries.SearchAirports;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Airports.Queries.SearchAirports
{
    public class SearchAirportsQueryHandlerTests
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IAirportCacheService _cache;

        private readonly SearchAirportsQueryHandler _handler;

        public SearchAirportsQueryHandlerTests()
        {
            _airportRepository = Substitute.For<IAirportRepository>();

            // Pass the factory delegate straight through so tests exercise real handler logic.
            _cache = Substitute.For<IAirportCacheService>();
            _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IReadOnlyList<GoVoylo.Application.Features.Airports.Dtos.AirportDto>>>>())
                .Returns(callInfo => callInfo.ArgAt<Func<Task<IReadOnlyList<GoVoylo.Application.Features.Airports.Dtos.AirportDto>>>>(1)());

            _handler = new SearchAirportsQueryHandler(_airportRepository, _cache);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedAirports_WhenRepositoryHasMatches()
        {
            var airport = new Airport("BOM", "Chhatrapati Shivaji Maharaj International Airport", "Mumbai", "India", true);

            _airportRepository
                .SearchAsync("mum", Arg.Any<int>())
                .Returns(new List<Airport> { airport });

            var result = await _handler.Handle(new SearchAirportsQuery("mum"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].IataCode.Should().Be("BOM");
            result[0].City.Should().Be("Mumbai");
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoMatches()
        {
            _airportRepository
                .SearchAsync(Arg.Any<string>(), Arg.Any<int>())
                .Returns(new List<Airport>());

            var result = await _handler.Handle(new SearchAirportsQuery("zzz"), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
