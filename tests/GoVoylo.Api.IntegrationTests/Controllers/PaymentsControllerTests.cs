// tests/GoVoylo.Api.IntegrationTests/Controllers/PaymentsControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using GoVoylo.Application.Features.Payments.Dtos;
using Xunit;
using GoVoylo.Infrastructure.Persistence.EntityFramework;

namespace GoVoylo.Api.IntegrationTests.Controllers;

public class PaymentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    // WebApplicationFactory spins up real Program.cs inside computer memory
    public PaymentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
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

    [Fact]
    public async Task Should_ProcessPayment_And_RetrieveViaQuery_Then_RollbackSuccessfully()
    {
        // Arrange
        // 1. Extract real DbContext out of the running WebApplicationFactory container
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var command = new ProcessPaymentCommand(
            BookingReference: "CQRS-HTTP-TEST-999",
            Amount: 1850.00m,
            Currency: "INR",
            SourceClient: "IntegrationTestSuite",
            PaymentMethodToken: "tok_visa_cqrs_valid"
        );

        // 2. Open an explicit transaction block on live PostgreSQL Docker container
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // --- 1. THE COMMAND (WRITE) PHASE ---
            // Act: Shoot a real HTTP POST request down the endpoint wire
            var postResponse = await _client.PostAsJsonAsync("api/payments", command);
            postResponse.EnsureSuccessStatusCode();

            // Deserialize the response object to grab the new primary tracking ID
            var commandResult = await postResponse.Content.ReadFromJsonAsync<PaymentResponseDto>();
            Assert.NotNull(commandResult);
            Assert.NotEqual(Guid.Empty, commandResult.Id);

            // --- 2. THE QUERY (READ) PHASE ---
            // Act: Shoot a real HTTP GET request to pull the uncommitted row back out
            // (Assuming GET endpoint is structured as: api/payments/{id})
            var getResponse = await _client.GetAsync($"api/payments/{commandResult.Id}");
            getResponse.EnsureSuccessStatusCode();

            var queryResult = await getResponse.Content.ReadFromJsonAsync<PaymentDetailsDto>();

            // --- 3. THE ASSERTION PHASE ---
            // Verify structural data integrity between original input and final database output
            Assert.NotNull(queryResult);
            Assert.Equal("CQRS-HTTP-TEST-999", queryResult.BookingReference);
            Assert.Equal(1850.00m, queryResult.TotalAmount);
            Assert.Equal("INR", queryResult.Currency);
        }
        finally
        {
            // --- 4. THE CLEANUP ---
            // Wipe the data from your Docker tables so your database remains pristine
            await transaction.RollbackAsync();
        }
    }

}
