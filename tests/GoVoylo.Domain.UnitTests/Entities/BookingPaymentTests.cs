// tests/GoVoylo.Domain.UnitTests/Entities/BookingPaymentTests.cs
using GoVoylo.Domain.Entities; 
using FluentAssertions;
using Xunit;

namespace GoVoylo.Domain.UnitTests.Entities;

public class BookingPaymentTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        string reference = "BOOK-12345";
        //decimal amount = 550.75m;
        decimal supplierAmount = 500.00m;
        decimal commissionAmount = 50.75m;
        decimal expectedTotalAmount = 550.75m;
        string currency = "USD";

        // Act
        var payment = new BookingPayment(reference,
            supplierAmount,
            commissionAmount,
            currency);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.BookingReference.Should().Be(reference);

        payment.SupplierAmount
            .Should()
            .Be(supplierAmount);

        payment.CommissionAmount
            .Should()
            .Be(commissionAmount);

        payment.TotalAmount
            .Should()
            .Be(expectedTotalAmount);

        payment.Currency
            .Should()
            .Be(currency);

        payment.PaymentStatus
            .Should()
            .Be("Pending");
    }

    [Fact]
    public void Constructor_WithZeroOrNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => new BookingPayment("BOOK-123",
            0m,
            50m,
            "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Payment amount must be greater than zero.");
    }
}
