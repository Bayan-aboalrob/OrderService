using MediatR;

namespace OrderService.Application.Queries
{
    public sealed record GetOrderByReservationIdQuery(Guid ReservationId) : IRequest<Order?>;
}

