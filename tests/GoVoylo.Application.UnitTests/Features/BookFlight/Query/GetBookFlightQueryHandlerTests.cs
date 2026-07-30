using FluentAssertions;
using GoVoylo.Application.Features.Booking.Query;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.UnitTests.Features.BookFlight.Query
{
    public class GetBookFlightQueryHandlerTests
    {
        private readonly IBookFlightRepository _bookFlightRepositoryMock;
        private readonly IActivityLogRepository _activityLogRepositoryMock;
        private readonly GetBookFlightQueryHandler _handler;

        public GetBookFlightQueryHandlerTests()
        {
            //arrange dependencies - creates fake implementation for testing
            _bookFlightRepositoryMock = Substitute.For<IBookFlightRepository>();

            _handler = new GetBookFlightQueryHandler(_bookFlightRepositoryMock);
        }
        [Fact]
        public async Task Handle_ValidBookingReference_ShouldReturnBooking()
        {
           // Arrange: creates simple booking for test
            var booking = new FlightBooking(
                "AI203",
                "John Doe",
                "Pune",
                "Delhi",
                new DateTime(2026, 08, 15),
                1);

            //a request sent to handler.
            var query = new GetBookFlightQuery(
                booking.FlightBookingReference);

            //Returns the booking when someone calls with the given booking reference.
            _bookFlightRepositoryMock
                .GetByBookingReferenceAsync(
                    booking.FlightBookingReference,
                    Arg.Any<CancellationToken>())
                .Returns(booking);

            // Act - execute
            var result = await _handler.Handle(
                query,
                CancellationToken.None);

            // Assert- verify 

            result.Should().NotBeNull();

            result.FlightBookingId
                .Should()
                .Be(booking.FlightBookingId);

            result.FlightBookingReference
                .Should()
                .Be(booking.FlightBookingReference);

            result.FlightNumber
                .Should()
                .Be(booking.FlightNumber);

            result.PassengerName
                .Should()
                .Be(booking.PassengerName);

            await _bookFlightRepositoryMock
                .Received(1)
                .GetByBookingReferenceAsync(
                    booking.FlightBookingReference,
                    Arg.Any<CancellationToken>());
        }
    }
}
