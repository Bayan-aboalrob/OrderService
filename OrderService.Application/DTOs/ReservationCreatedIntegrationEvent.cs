namespace OrderService.Application.Dtos
{
    public sealed class ReservationCreatedIntegrationEvent
    {
        public Guid Id { get; set; }         
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiryTimeUtc { get; set; }
        public string? CorrelationId { get; set; }
    }
}
