using System.Threading;
using System.Threading.Tasks;
using FlashSaleDB.Entities;
using MediatR;
using OrderService.Application.Contracts;
using OrderService.Application.Queries;

namespace OrderService.Application.Handlers
{
    public sealed class GetOrderByReservationIdHandler
        : IRequestHandler<GetOrderByReservationIdQuery, Order?>
    {
        private readonly IOrderRepository _orders;

        public GetOrderByReservationIdHandler(IOrderRepository orders)
        {
            _orders = orders;
        }

        public Task<Order?> Handle(GetOrderByReservationIdQuery request, CancellationToken ct)
            => _orders.GetByReservationIdAsync(request.ReservationId, ct);
    }
}
