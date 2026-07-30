// tests/GoVoylo.Api.IntegrationTests/Controllers/PaymentsControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using GoVoylo.Application.Features.Payments.Dtos;
using Xunit;

namespace GoVoylo.Api.IntegrationTests.Controllers;

public class PaymentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    // WebApplicationFactory spins up your real Program.cs inside computer memory
    public PaymentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProcessPayment_WithValidPayload_ShouldRouteThroughCleanArchitectureAndReturnSuccess()
    {
        // Arrange - Mimicking an incoming payload from a Python AI Agent or Mobile App
        var command = new ProcessPaymentCommand(
            BookingReference: "INT-TEST-777",
            Amount: 1250.50m,
            Currency: "EUR",
            SourceClient: "PythonAiAgent",
            PaymentMethodToken: "tok_integration_testing_valid"
        );

        // Act - Shoot a real HTTP POST request down the endpoint wire
        var response = await _client.PostAsJsonAsync("api/payments", command);

        // Assert - 1. Verify HTTP Layer Routing and Security Middleware passed
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - 2. Verify Data Layer Payload was generated and returned flawlessly
        var result = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
        result.Should().NotBeNull();
        result!.BookingReference.Should().Be(command.BookingReference);
        result.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be("EUR");
        result.Status.Should().Be("Pending");
        result.TransactionId.Should().NotBeEmpty();
    }
}
