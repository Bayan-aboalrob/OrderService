namespace OrderService.Application.Dtos
{
    public sealed record CreateOrderFromReservationRequest(
        Guid UserId,
        Guid CartId,
        Guid ReservationId,
        string? CorrelationId,
        decimal? Total  
    );

    public sealed record CreateOrderFromReservationResponse(
        Guid OrderId,
        Guid UserId,
        string OrderStatus
    );
}
