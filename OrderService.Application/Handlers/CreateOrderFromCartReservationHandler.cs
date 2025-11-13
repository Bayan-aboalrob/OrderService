using MediatR;
using OrderService.Application.Contracts;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class CreateOrderFromCartReservationHandler
        : IRequestHandler<CreateOrderFromCartReservationCommand, Guid>
    {
        private readonly IOrderRepository _orders;
        private readonly IProductPricingService _pricing;
        private readonly IBusPublisher _bus;

        public CreateOrderFromCartReservationHandler(
            IOrderRepository orders,
            IProductPricingService pricing,
            IBusPublisher bus)
        {
            _orders = orders;
            _pricing = pricing;
            _bus = bus;
        }

        public async Task<Guid> Handle(CreateOrderFromCartReservationCommand request, CancellationToken ct)
        {
            if (await _orders.ExistsForCartAsync(request.CartId, ct))
                return Guid.Empty;

            if (request.Items is null || request.Items.Count == 0)
                return Guid.Empty;

            decimal total = 0m;

            foreach (var item in request.Items)
            {
                var (price, discount) = await _pricing.GetPriceAsync(item.ProductId, ct);
                var effective = price - (price * discount);
                total += effective * item.Quantity;
            }

            var firstReservationId = request.Items.First().ReservationId;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CartId = request.CartId,
                Total = total,
                OrderStatus = "PendingPayment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                ReservationId = firstReservationId,
                CorrelationId = request.CorrelationId
            };

            await _orders.AddAsync(order, ct);
            await _orders.SaveChangesAsync(ct);

            await _bus.PublishAsync("Order.Created", new
            {
                order.Id,
                order.CartId,
                order.UserId,
                order.Total,
                order.OrderStatus,
                order.CorrelationId
            }, ct);

            return order.Id;
        }
    }
}
