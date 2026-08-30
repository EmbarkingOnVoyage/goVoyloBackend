using FluentAssertions;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Airports.Queries.GetAirport;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Airports.Queries.GetAirport
{
    public class GetAirportQueryHandlerTests
    {
        private readonly IAirportRepository _airportRepository;
        private readonly GetAirportQueryHandler _handler;

        public GetAirportQueryHandlerTests()
        {
            _airportRepository = Substitute.For<IAirportRepository>();
            _handler = new GetAirportQueryHandler(_airportRepository);
        }

        [Fact]
        public async Task Handle_ShouldReturnAirport_WhenIataCodeExists()
        {
            var airport = new Airport("DEL", "Indira Gandhi International Airport", "Delhi", "India", true);

            _airportRepository.GetByIataAsync("DEL").Returns(airport);

            var result = await _handler.Handle(new GetAirportQuery("DEL"), CancellationToken.None);

            result.IataCode.Should().Be("DEL");
            result.City.Should().Be("Delhi");
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenIataCodeDoesNotExist()
        {
            _airportRepository.GetByIataAsync("ZZZ").Returns((Airport?)null);

            var act = () => _handler.Handle(new GetAirportQuery("ZZZ"), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
