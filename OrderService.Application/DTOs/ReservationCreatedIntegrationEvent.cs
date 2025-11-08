using System.Text.Json.Serialization;

namespace OrderService.Application.Dtos
{
    public sealed class ReservationCreatedIntegrationEvent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }
        
        [JsonPropertyName("cartId")]
        public Guid CartId { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("expiryTimeUtc")]
        public DateTime ExpiryTimeUtc { get; set; }

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }
    }
}
