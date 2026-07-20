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
        decimal amount = 550.75m;
        string currency = "USD";

        // Act
        var payment = new BookingPayment(reference, amount, currency);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.BookingReference.Should().Be(reference);
        payment.TotalAmount.Should().Be(amount);
        payment.Currency.Should().Be(currency);
        payment.PaymentStatus.Should().Be("Pending");
    }

    [Fact]
    public void Constructor_WithZeroOrNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => new BookingPayment("BOOK-123", 0m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Payment amount must be greater than zero.");
    }
}
