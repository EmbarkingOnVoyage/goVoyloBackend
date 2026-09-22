using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GoVoylo.Infrastructure.ExternalServices.Cashfree
{
    public class CashfreePaymentGateway : IPaymentProvider
    {
        private readonly HttpClient _httpClient;
        private readonly CashfreeOptions _options;

        public string ProviderName =>
            CashfreeConstants.ProviderName;

        public CashfreePaymentGateway(
            HttpClient httpClient,
            IOptions<CashfreeOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PaymentOrderResult> CreateOrderAsync(
            PaymentOrderRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId))
            {
                throw new InvalidOperationException(
                    "Cashfree ClientId is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.ClientSecret))
            {
                throw new InvalidOperationException(
                    "Cashfree ClientSecret is not configured.");
            }

            if (!string.Equals(
                    request.Currency,
                    "INR",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Cashfree currently supports INR only for this integration.");
            }

            var expectedTotal =
                request.SupplierAmount +
                request.CommissionAmount;

            if (request.TotalAmount != expectedTotal)
            {
                throw new InvalidOperationException(
                    "Total amount must equal supplier amount plus commission amount.");
            }

            var orderId =
                request.BookingReference;

            var cashfreeRequest =
                new CashfreeCreateOrderRequest(
                    order_id: orderId,
                    order_amount: request.TotalAmount,
                    order_currency: request.Currency.ToUpperInvariant(),
                    customer_details:
                        new CashfreeCustomerDetails(
                            customer_id: request.BookingReference,
                            customer_phone: "9999999999"
                        )
                );

            var json =
                JsonSerializer.Serialize(cashfreeRequest);

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{GetBaseUrl()}/orders");

            httpRequest.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            httpRequest.Headers.Add(
                "x-client-id",
                _options.ClientId);

            httpRequest.Headers.Add(
                "x-client-secret",
                _options.ClientSecret);

            httpRequest.Headers.Add(
                "x-api-version",
                _options.ApiVersion);

            httpRequest.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            using var response =
                await _httpClient.SendAsync(
                    httpRequest,
                    cancellationToken);

            var responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Cashfree API Error " +
                    $"{(int)response.StatusCode}: {responseContent}");
            }

            var cashfreeResponse =
                JsonSerializer.Deserialize<CashfreeCreateOrderResponse>(
                    responseContent);

            if (cashfreeResponse == null ||
                string.IsNullOrWhiteSpace(
                    cashfreeResponse.payment_session_id))
            {
                throw new InvalidOperationException(
                    "Invalid response received from Cashfree.");
            }

            return new PaymentOrderResult(
                OrderId: cashfreeResponse.order_id,
                Amount: cashfreeResponse.order_amount,
                Currency: cashfreeResponse.order_currency,
                PublicKey: null,
                CheckoutToken:
                cashfreeResponse.payment_session_id);
        }

        public async Task<IPaymentProvider.PaymentVerificationResult>
            VerifyPaymentAsync(
                string orderId,
                string paymentId,
                string? signature,
                CancellationToken cancellationToken)
        {
            throw new NotImplementedException(
                "Cashfree payment verification will be implemented next.");
        }

        private string GetBaseUrl()
        {
            return string.Equals(
                _options.Environment,
                "production",
                StringComparison.OrdinalIgnoreCase)
                ? CashfreeConstants.ProductionBaseUrl
                : CashfreeConstants.SandboxBaseUrl;
        }
    }
}
