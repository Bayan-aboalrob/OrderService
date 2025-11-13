using System.Text.Json.Serialization;

namespace OrderService.Application.Dtos
{
    public sealed class PaymentSucceededIntegrationEvent
    {
        [JsonPropertyName("id")]
        public Guid PaymentId { get; set; }

        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Succeeded";

        [JsonPropertyName("correlationId")]
        public Guid? CorrelationId { get; set; }
    }
}
