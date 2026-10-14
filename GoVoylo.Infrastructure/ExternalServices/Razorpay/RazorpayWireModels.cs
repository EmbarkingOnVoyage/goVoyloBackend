using System.Text.Json.Serialization;

namespace GoVoylo.Infrastructure.ExternalServices.Razorpay
{
    public class CreateOrderRequestWire
    {
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "INR";

        [JsonPropertyName("receipt")]
        public string Receipt { get; set; } = string.Empty;

        [JsonPropertyName("payment_capture")]
        public int PaymentCapture { get; set; } = 1;
    }

    public class CreateOrderResponseWire
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    // GET /payments/{id}?expand[]=card — only the fields that say how it was paid.
    public class PaymentResponseWire
    {
        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("bank")]
        public string? Bank { get; set; }

        [JsonPropertyName("wallet")]
        public string? Wallet { get; set; }

        [JsonPropertyName("card")]
        public PaymentCardWire? Card { get; set; }
    }

    public class PaymentCardWire
    {
        [JsonPropertyName("network")]
        public string? Network { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }
    }
}
