using System.Text.Json.Serialization;

namespace OrderService.Application.Dtos
{
    public sealed class ReservationCartCreatedIntegrationEvent
    {
        [JsonPropertyName("cartId")]
        public Guid CartId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("expiresAtUtc")]
        public DateTime ExpiresAtUtc { get; set; }

        [JsonPropertyName("reservations")]
        public List<ReservationCartItem> Reservations { get; set; } = new();
    }

    public sealed class ReservationCartItem
    {
        [JsonPropertyName("reservationId")]
        public Guid ReservationId { get; set; }

        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("expiryTimeUtc")]
        public DateTime ExpiryTimeUtc { get; set; }
    }
}
