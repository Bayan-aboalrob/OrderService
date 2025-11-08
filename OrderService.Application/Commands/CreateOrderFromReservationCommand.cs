using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public sealed record CreateOrderFromReservationCommand(
        Guid ReservationId,
        Guid UserId,
        Guid ProductId,
        Guid CartId,
        int Quantity,
        DateTime ExpiryTimeUtc,
        string? CorrelationId
    ) : IRequest<Guid>;
}
