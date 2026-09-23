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
}
