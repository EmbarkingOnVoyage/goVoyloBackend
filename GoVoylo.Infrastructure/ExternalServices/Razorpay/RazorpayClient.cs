using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{
    public class RazorpayClient : IRazorpayClient
    {
        private readonly HttpClient _httpClient;
        private readonly RazorpayOptions _options;

        public RazorpayClient(HttpClient httpClient, IOptions<RazorpayOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public string KeyId => _options.KeyId;

        public async Task<RazorpayOrderResult> CreateOrderAsync(
            decimal amount, string currency, string receipt, CancellationToken cancellationToken)
        {
            // Razorpay wants the smallest currency unit (paise for INR), not rupees.
            var amountInSmallestUnit = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

            var wireRequest = new CreateOrderRequestWire
            {
                Amount = amountInSmallestUnit,
                Currency = currency,
                Receipt = receipt,
            };

            using var httpResponse = await _httpClient.PostAsJsonAsync("orders", wireRequest, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Razorpay order creation failed ({httpResponse.StatusCode}): {body}");
            }

            var wireResponse = await httpResponse.Content.ReadFromJsonAsync<CreateOrderResponseWire>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Razorpay order creation returned an empty response.");

            return new RazorpayOrderResult(wireResponse.Id, wireResponse.Amount, wireResponse.Currency);
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            var payload = $"{orderId}|{paymentId}";
            var keyBytes = Encoding.UTF8.GetBytes(_options.KeySecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var expectedHash = hmac.ComputeHash(payloadBytes);
            var expectedSignature = Convert.ToHexString(expectedHash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature));
        }
    }
}
