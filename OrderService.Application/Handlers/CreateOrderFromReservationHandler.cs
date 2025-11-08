using FlashSaleDB.Entities;
using MediatR;
using OrderService.Application.Contracts;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class CreateOrderFromReservationHandler
        : IRequestHandler<CreateOrderFromReservationCommand, Guid>
    {
        private readonly IOrderRepository _orders;
        private readonly IProductPricingService _pricing;
        private readonly IBusPublisher _bus;

        public CreateOrderFromReservationHandler(
            IOrderRepository orders,
            IProductPricingService pricing,
            IBusPublisher bus)
        {
            _orders = orders;
            _pricing = pricing;
            _bus = bus;
        }

        public async Task<Guid> Handle(CreateOrderFromReservationCommand request, CancellationToken ct)
        {
            if (await _orders.ExistsForReservationAsync(request.ReservationId, ct))
                return Guid.Empty;

            var (price, discount) = await _pricing.GetPriceAsync(request.ProductId, ct);

            var discountedPrice = price - (price * discount);
            var total = discountedPrice * request.Quantity;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Total = total,
                CartId = request.CartId,
                OrderStatus = "PendingPayment",
                CreatedAt = DateTime.UtcNow,
                ReservationId = request.ReservationId,
                CorrelationId = request.CorrelationId
            };

            await _orders.AddAsync(order, ct);
            await _orders.SaveChangesAsync(ct);

            await _bus.PublishAsync("Order.Created", new
            {
                order.Id,
                order.ReservationId,
                order.UserId,
                order.CartId,
                request.ProductId,
                request.Quantity,
                order.Total,
                order.CorrelationId
            }, ct);

            return order.Id;
        }
    }
}
