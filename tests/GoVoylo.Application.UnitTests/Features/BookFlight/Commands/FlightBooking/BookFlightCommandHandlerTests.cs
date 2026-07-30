using FluentAssertions;
using GoVoylo.Application.Features.Booking.Commands;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.UnitTests.Features.BookFlight.Commands.BookFlightCommandHandlerTests
{
    public class BookFlightCommandHandlerTests
    {
        private readonly IBookFlightRepository _bookingRepositoryMock;
        private readonly IActivityLogRepository _activityLogRepositoryMock;
        private readonly BookFlightCommandHandler _handler;
        public BookFlightCommandHandlerTests()
        {
            // 1. Create fake implementations of our domain interfaces using NSubstitute
            _bookingRepositoryMock = Substitute.For<IBookFlightRepository>();
            _activityLogRepositoryMock = Substitute.For<IActivityLogRepository>();

            // 2. Inject the mocks into our real Handler (SOLID Dependency Inversion in action!)
            _handler = new BookFlightCommandHandler(_bookingRepositoryMock);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldBookFlightSuccessfully()
        {
            // Arrange: 
            var command = new BookFlightCommand(
                FlightNumber: "AI203",
                PassengerName: "John Doe",
                From: "Pune",
                To: "Delhi",
                JourneyDate: new DateTime(2026, 08, 15),
                NumberOfPassengers: 1);

            _bookingRepositoryMock
                .SaveAsync(
                    Arg.Any<FlightBooking>(),
                    Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            result.FlightNumber.Should().Be(command.FlightNumber);
            result.PassengerName.Should().Be(command.PassengerName);

            await _bookingRepositoryMock.Received(1)
                .SaveAsync(
                    Arg.Any<GoVoylo.Domain.Entities.FlightBooking>(),
                    Arg.Any<CancellationToken>());
        }

        //Hthis throws an exception when the value is not stored in real db
        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var command = new BookFlightCommand(
                "AI203",
                "John Doe",
                "Pune",
                "Delhi",
                new DateTime(2026, 08, 15),
                1);

            _bookingRepositoryMock
                .When(x => x.SaveAsync(
                    Arg.Any<GoVoylo.Domain.Entities.FlightBooking>(),
                    Arg.Any<CancellationToken>()))
                .Do(_ => throw new Exception("Database Error"));


            // Act
            Func<Task> act = async () =>
                await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }
        public async Task Handle_ValidCommand_ShouldGetBookFlightSuccessfully()
        {
            // Arrange
            

            //Act


            //Assert

        }
    }
}
