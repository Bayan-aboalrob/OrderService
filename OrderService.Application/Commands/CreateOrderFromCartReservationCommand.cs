using MediatR;
using OrderService.Application.Dtos;

namespace OrderService.Application.Orders.Commands
{
    public sealed record CreateOrderFromCartReservationCommand(
        Guid CartId,
        Guid UserId,
        DateTime ExpiresAtUtc,
        string? CorrelationId,
        IReadOnlyCollection<ReservationCartItem> Items
    ) : IRequest<Guid>;
}
