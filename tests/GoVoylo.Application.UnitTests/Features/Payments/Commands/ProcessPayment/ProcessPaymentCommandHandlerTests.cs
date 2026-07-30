// tests/GoVoylo.Application.UnitTests/Features/Payments/Commands/ProcessPayment/ProcessPaymentCommandHandlerTests.cs
using FluentAssertions;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace GoVoylo.Application.UnitTests.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepositoryMock;
    private readonly IActivityLogRepository _activityLogRepositoryMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        // 1. Create fake implementations of our domain interfaces using NSubstitute
        _paymentRepositoryMock = Substitute.For<IPaymentRepository>();
        _activityLogRepositoryMock = Substitute.For<IActivityLogRepository>();

        // 2. Inject the mocks into our real Handler (SOLID Dependency Inversion in action!)
        _handler = new ProcessPaymentCommandHandler(_paymentRepositoryMock, _activityLogRepositoryMock);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSavePaymentAndLogActivity()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            BookingReference: "BK-9988",
            Amount: 250.00m,
            Currency: "USD",
            SourceClient: "AiAgent",
            PaymentMethodToken: "tok_visa_testing"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify the returned DTO properties are correct
        result.Should().NotBeNull();
        result.BookingReference.Should().Be(command.BookingReference);
        result.Amount.Should().Be(command.Amount);
        result.Status.Should().Be("Pending");

        // Assert - Verify the database interactions actually occurred exactly once [1]
        await _paymentRepositoryMock
            .Received(1)
            .SaveAsync(Arg.Is<BookingPayment>(p => p.BookingReference == command.BookingReference && p.TotalAmount == command.Amount));

        await _activityLogRepositoryMock
            .Received(1)
            .LogActivityAsync(Arg.Is<UserActivityLog>(l => l.SourcePlatform == "AiAgent" && l.ActionType == "PaymentInitiated"));
    }
}
