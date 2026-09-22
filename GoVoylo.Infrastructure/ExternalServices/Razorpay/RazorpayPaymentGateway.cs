using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{ 
    public class RazorpayPaymentGateway : IPaymentProvider
    {
        private readonly HttpClient _httpClient;
        private readonly RazorpayOptions _options;

        public string ProviderName =>
            RazorpayConstants.ProviderName;

        public RazorpayPaymentGateway(
            HttpClient httpClient,
            IOptions<RazorpayOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PaymentOrderResult> CreateOrderAsync(
            PaymentOrderRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.KeyId))
                throw new InvalidOperationException(
                    "Razorpay KeyId is not configured.");

            if (string.IsNullOrWhiteSpace(_options.KeySecret))
                throw new InvalidOperationException(
                    "Razorpay KeySecret is not configured.");

            //if (string.IsNullOrWhiteSpace(_options.CommissionAccountId))
            //    throw new InvalidOperationException(
            //        "Razorpay CommissionAccountId is not configured.");

            //if (string.IsNullOrWhiteSpace(_options.SupplierAccountId))
            //    throw new InvalidOperationException(
            //        "Razorpay SupplierAccountId is not configured.");

            if (_options.EnableTransfers)
            {
                if (string.IsNullOrWhiteSpace(_options.CommissionAccountId))
                    throw new InvalidOperationException(
                        "Razorpay CommissionAccountId is not configured.");

                if (string.IsNullOrWhiteSpace(_options.SupplierAccountId))
                    throw new InvalidOperationException(
                        "Razorpay SupplierAccountId is not configured.");
            }

            if (!string.Equals(
                    request.Currency,
                    "INR",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Razorpay split payment currently supports INR only.");
            }

            var expectedTotal =
                request.SupplierAmount +
                request.CommissionAmount;

            if (request.TotalAmount != expectedTotal)
            {
                throw new InvalidOperationException(
                    "Total amount must equal supplier amount plus commission amount.");
            }

            var currency =
                request.Currency.ToUpperInvariant();

            // Razorpay accepts amount in paise.
            var totalAmountInPaise =
                (int)Math.Round(request.TotalAmount * 100);

            var supplierAmountInPaise =
                (int)Math.Round(request.SupplierAmount * 100);

            var commissionAmountInPaise =
                (int)Math.Round(request.CommissionAmount * 100);

            //var razorpayRequest =
            //    new RazorpayCreateOrderRequest(
            //        amount: totalAmountInPaise,
            //        currency: currency,
            //        receipt: request.BookingReference,
            //        transfers: new object[]
            //        {
            //        new
            //        {
            //            account = _options.SupplierAccountId,
            //            amount = supplierAmountInPaise,
            //            currency = currency,
            //            notes = new
            //            {
            //                type = "SupplierPayment",
            //                bookingReference =
            //                    request.BookingReference
            //            }
            //        },
            //        new
            //        {
            //            account = _options.CommissionAccountId,
            //            amount = commissionAmountInPaise,
            //            currency = currency,
            //            notes = new
            //            {
            //                type = "GoVoyloCommission",
            //                bookingReference =
            //                    request.BookingReference
            //            }
            //        }
            //        });

            //    object razorpayRequest;

            //    if (_options.EnableTransfers)
            //    {
            //       var razorpayRequest = new
            //        {
            //            amount = totalAmountInPaise,
            //            currency = currency,
            //            receipt = request.BookingReference,
            //            transfers = new[]
            //            {
            //    new
            //    {
            //        account = _options.SupplierAccountId,
            //        amount = supplierAmountInPaise,
            //        currency = currency,
            //        notes = new
            //        {
            //            type = "SupplierPayment",
            //            bookingReference = request.BookingReference
            //        }
            //    },
            //    new
            //    {
            //        account = _options.CommissionAccountId,
            //        amount = commissionAmountInPaise,
            //        currency = currency,
            //        notes = new
            //        {
            //            type = "GoVoyloCommission",
            //            bookingReference = request.BookingReference
            //        }
            //    }
            //}
            //        };
            //    }
            //    else
            //    {
            //       var razorpayRequest = new
            //        {
            //            amount = totalAmountInPaise,
            //            currency = currency,
            //            receipt = request.BookingReference
            //        };
            //    }

            var razorpayRequest = new RazorpayCreateOrderRequest(
     amount: totalAmountInPaise,
     currency: currency,
     receipt: request.BookingReference,
     transfers: _options.EnableTransfers
         ? new[]
         {
            new RazorpayTransfer(
                account: _options.SupplierAccountId!,
                amount: supplierAmountInPaise,
                currency: currency,
                notes: new
                {
                    type = "SupplierPayment",
                    bookingReference = request.BookingReference
                }
            ),
            new RazorpayTransfer(
                account: _options.CommissionAccountId!,
                amount: commissionAmountInPaise,
                currency: currency,
                notes: new
                {
                    type = "GoVoyloCommission",
                    bookingReference = request.BookingReference
                }
            )
         }
         : null
 );


            var json =
                JsonSerializer.Serialize(razorpayRequest);

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    RazorpayConstants.OrdersEndpoint);

            httpRequest.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            var credentials =
                $"{_options.KeyId}:{_options.KeySecret}";

            var encodedCredentials =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(credentials));

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    encodedCredentials);

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
                    $"Razorpay API Error " +
                    $"{(int)response.StatusCode}: {responseContent}");
            }

            var razorpayResponse =
                JsonSerializer.Deserialize<RazorpayCreateOrderResponse>(
                    responseContent);

            if (razorpayResponse == null ||
                string.IsNullOrWhiteSpace(razorpayResponse.id))
            {
                throw new InvalidOperationException(
                    "Invalid response received from Razorpay.");
            }

            return new PaymentOrderResult(
                OrderId: razorpayResponse.id,
                Amount: razorpayResponse.amount / 100m,
                Currency: razorpayResponse.currency,
                PublicKey: _options.KeyId,
                CheckoutToken: null);
        }

        public async Task<IPaymentProvider.PaymentVerificationResult>
            VerifyPaymentAsync(
                string orderId,
                string paymentId,
                string? signature,
                CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.KeyId))
                throw new InvalidOperationException(
                    "Razorpay KeyId is not configured.");

            if (string.IsNullOrWhiteSpace(_options.KeySecret))
                throw new InvalidOperationException(
                    "Razorpay KeySecret is not configured.");

            if (string.IsNullOrWhiteSpace(signature))
            {
                return new IPaymentProvider.PaymentVerificationResult(
                    false,
                    null,
                    "Failed");
            }

            var payload =
                $"{orderId}|{paymentId}";

            using var hmac =
                new HMACSHA256(
                    Encoding.UTF8.GetBytes(
                        _options.KeySecret));

            var hash =
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(payload));

            var generatedSignature =
                Convert.ToHexString(hash)
                    .ToLowerInvariant();

            var isValid =
                CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(generatedSignature),
                    Encoding.UTF8.GetBytes(signature));

            if (!isValid)
            {
                return new IPaymentProvider.PaymentVerificationResult(
                    false,
                    null,
                    "Failed");
            }

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{RazorpayConstants.PaymentsEndpoint}/{paymentId}");

            var credentials =
                $"{_options.KeyId}:{_options.KeySecret}";

            var encodedCredentials =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(credentials));

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    encodedCredentials);

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
                    $"Razorpay Payment API Error " +
                    $"{(int)response.StatusCode}: {responseContent}");
            }

            using var document =
                JsonDocument.Parse(responseContent);

            var root =
                document.RootElement;

            var paymentMethod =
                root.TryGetProperty(
                    "method",
                    out var methodProperty)
                        ? methodProperty.GetString()
                        : null;

            var paymentStatus =
                root.TryGetProperty(
                    "status",
                    out var statusProperty)
                        ? statusProperty.GetString()
                        : null;

            return new IPaymentProvider.PaymentVerificationResult(
                true,
                paymentMethod,
                paymentStatus ?? "unknown");
        }
    }
}