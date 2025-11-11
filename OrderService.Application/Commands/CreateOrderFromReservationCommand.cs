using System;
using MediatR;
using OrderService.Application.Dtos;

namespace OrderService.Application.Orders.Commands
{
    public sealed record CreateOrderFromReservationCommand(
        Guid UserId,
        Guid CartId,
        Guid ReservationId,
        string? CorrelationId,
        decimal? Total
    ) : IRequest<CreateOrderFromReservationResponse>;
}
