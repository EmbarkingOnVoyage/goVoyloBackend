using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static GoVoylo.Application.Interfaces.IPaymentProvider;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{
    public class RazorpayService : IPaymentProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public string ProviderName => "Razorpay";


        public RazorpayService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaymentOrderResult> CreateOrderAsync(
            decimal amount,
            string currency,
            string bookingReference,
            CancellationToken cancellationToken)
        {
            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];

            //Console.WriteLine($"Razorpay Key ID: {keyId}");
            //Console.WriteLine($"Razorpay Secret exists: {!string.IsNullOrWhiteSpace(keySecret)}");

            var amountInPaise = (int)(amount * 100);

            var requestBody = new
            {
                amount = amountInPaise,
                currency = currency,
                receipt = bookingReference
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.razorpay.com/v1/orders");

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            //response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

                throw new HttpRequestException(
                    $"Razorpay API Error {(int)response.StatusCode}: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(
                cancellationToken);

            using var document = JsonDocument.Parse(responseJson);

            var root = document.RootElement;

            var orderId = root.GetProperty("id").GetString()!;

            var razorpayAmount = root.GetProperty("amount").GetDecimal();

            var razorpayCurrency = root.GetProperty("currency").GetString()!;

            return new PaymentOrderResult(
                orderId,
                razorpayAmount / 100,
                razorpayCurrency);
        }

        //public bool VerifyPaymentSignature(
        //    string orderId,
        //    string paymentId,
        //    string signature)
        //{
        //    var keySecret = _configuration["Razorpay:KeySecret"];

        //    var payload = $"{orderId}|{paymentId}";

        //    using var hmac = new HMACSHA256(
        //        Encoding.UTF8.GetBytes(keySecret!));

        //    var hash = hmac.ComputeHash(
        //        Encoding.UTF8.GetBytes(payload));

        //    var generatedSignature =
        //        Convert.ToHexString(hash).ToLowerInvariant();

        //    return CryptographicOperations.FixedTimeEquals(
        //        Encoding.UTF8.GetBytes(generatedSignature),
        //        Encoding.UTF8.GetBytes(signature));
        //}
        //public Task<bool> VerifyPaymentAsync(
        //       string orderId,
        //        string paymentId,
        //        string signature,
        //        CancellationToken cancellationToken)
        //{
        //    var keySecret = _configuration["Razorpay:KeySecret"];

        //    if (string.IsNullOrWhiteSpace(keySecret))
        //    {
        //        throw new InvalidOperationException(
        //            "Razorpay KeySecret is not configured.");
        //    }

        //    var payload = $"{orderId}|{paymentId}";

        //    using var hmac = new HMACSHA256(
        //        Encoding.UTF8.GetBytes(keySecret));

        //    var hash = hmac.ComputeHash(
        //        Encoding.UTF8.GetBytes(payload));

        //    var generatedSignature =
        //        Convert.ToHexString(hash).ToLowerInvariant();

        //    var isValid =
        //        CryptographicOperations.FixedTimeEquals(
        //            Encoding.UTF8.GetBytes(generatedSignature),
        //            Encoding.UTF8.GetBytes(signature));

        //    return Task.FromResult(isValid);
        //}

        public async Task<IPaymentProvider.PaymentVerificationResult> VerifyPaymentAsync(
    string orderId,
    string paymentId,
    string signature,
    CancellationToken cancellationToken)
        {
            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) ||
                string.IsNullOrWhiteSpace(keySecret))
            {
                throw new InvalidOperationException(
                    "Razorpay credentials are not configured.");
            }

            // 1. Verify Razorpay payment signature
            var payload = $"{orderId}|{paymentId}";

            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(keySecret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(payload));

            var generatedSignature =
                Convert.ToHexString(hash).ToLowerInvariant();

            var isValid =
                CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(generatedSignature),
                    Encoding.UTF8.GetBytes(signature));

            // Payment signature is invalid
            if (!isValid)
            {
                return new IPaymentProvider.PaymentVerificationResult(
                    false,
                    null);
            }

            // 2. Get payment details from Razorpay
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.razorpay.com/v1/payments/{paymentId}");

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    credentials);

            var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                throw new HttpRequestException(
                    $"Razorpay Payment API Error {(int)response.StatusCode}: {errorBody}");
            }

            // 3. Read Razorpay payment response
            var responseJson =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document =
                JsonDocument.Parse(responseJson);

            var root = document.RootElement;

            // 4. Get payment method
            var paymentMethod =
                root.TryGetProperty("method", out var methodProperty)
                    ? methodProperty.GetString()
                    : null;

            // 5. Return verification result
            return new IPaymentProvider.PaymentVerificationResult(
                true,
                paymentMethod);
        }
    }
}
