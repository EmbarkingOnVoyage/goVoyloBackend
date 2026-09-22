using FluentAssertions;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace GoVoylo.Application.UnitTests.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepositoryMock;
    private readonly IActivityLogRepository _activityLogRepositoryMock;
    private readonly IPaymentProviderResolver _providerResolverMock;
    private readonly IPaymentProvider _paymentProviderMock;

    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        // Mock dependencies
        _paymentRepositoryMock =
            Substitute.For<IPaymentRepository>();

        _activityLogRepositoryMock =
            Substitute.For<IActivityLogRepository>();

        _providerResolverMock =
            Substitute.For<IPaymentProviderResolver>();

        _paymentProviderMock =
            Substitute.For<IPaymentProvider>();

        // Configure payment provider mock
        _paymentProviderMock.ProviderName
            .Returns("Razorpay");

        _paymentProviderMock
            .CreateOrderAsync(
                Arg.Any<PaymentOrderRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                new PaymentOrderResult(
                    OrderId: "order_test_123",
                    Amount: 5400m,
                    Currency: "INR",
                    PublicKey: "rzp_test_123",
                    CheckoutToken: null));

        // Configure provider resolver
        _providerResolverMock
            .GetProvider("Razorpay")
            .Returns(_paymentProviderMock);

        // Create handler
        _handler = new ProcessPaymentCommandHandler(
            _paymentRepositoryMock,
            _activityLogRepositoryMock,
            _providerResolverMock);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSavePaymentAndLogActivity()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            BookingReference: "BK-9988",
            BaseFare: 5000.00m,
            Commission: 400.00m,
            Currency: "INR",
            PaymentProvider: "Razorpay",
            SourceClient: "AiAgent"
        );

        // Expected values
        var expectedSupplierAmount = 5000.00m;
        var expectedCommissionAmount = 400.00m;
        var expectedCustomerPayableAmount = 5400.00m;

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert - Response
        result.Should().NotBeNull();

        result.BookingReference
            .Should()
            .Be(command.BookingReference);

        result.SupplierAmount
            .Should()
            .Be(expectedSupplierAmount);

        result.CommissionAmount
            .Should()
            .Be(expectedCommissionAmount);

        result.CustomerPayableAmount
            .Should()
            .Be(expectedCustomerPayableAmount);

        result.Currency
            .Should()
            .Be(command.Currency);

        result.PaymentProvider
            .Should()
            .Be("Razorpay");

        result.ProviderOrderId
            .Should()
            .Be("order_test_123");

        result.PublicKey
            .Should()
            .Be("rzp_test_123");

        result.CheckoutToken
            .Should()
            .BeNull();

        // Assert - Payment saved exactly once
        await _paymentRepositoryMock
            .Received(1)
            .SaveAsync(
                Arg.Is<BookingPayment>(
                    p =>
                        p.BookingReference == command.BookingReference &&
                        p.SupplierAmount == expectedSupplierAmount &&
                        p.CommissionAmount == expectedCommissionAmount &&
                        p.TotalAmount == expectedCustomerPayableAmount &&
                        p.Currency == command.Currency &&
                        p.PaymentProvider == "Razorpay" &&
                        p.ProviderOrderId == "order_test_123"
                ));

        // Assert - Activity log created exactly once
        await _activityLogRepositoryMock
            .Received(1)
            .LogActivityAsync(
                Arg.Is<UserActivityLog>(
                    l =>
                        l.SourcePlatform == "AiAgent" &&
                        l.ActionType == "PaymentInitiated"),
                Arg.Any<CancellationToken>());

        // Assert - Correct provider was requested
        _providerResolverMock
            .Received(1)
            .GetProvider("Razorpay");

        // Assert - Generic payment order was created
        await _paymentProviderMock
            .Received(1)
            .CreateOrderAsync(
                Arg.Is<PaymentOrderRequest>(
                    r =>
                        r.BookingReference == command.BookingReference &&
                        r.TotalAmount == expectedCustomerPayableAmount &&
                        r.SupplierAmount == expectedSupplierAmount &&
                        r.CommissionAmount == expectedCommissionAmount &&
                        r.Currency == command.Currency
                ),
                Arg.Any<CancellationToken>());
    }
}