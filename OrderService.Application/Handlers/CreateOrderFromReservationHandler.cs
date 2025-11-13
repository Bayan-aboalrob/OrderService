using MediatR;
using OrderService.Application.Contracts;
using OrderService.Application.Dtos;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Services;

namespace OrderService.Application.Orders.Handlers
{
    /// <summary>
    /// Creates an order from a single reservation in a synchronous scenario.
    /// This is the version I'll call right after the reservation HTTP call to compare
    /// sync vs async performance.
    /// </summary>
    public sealed class CreateOrderFromReservationHandler
        : IRequestHandler<CreateOrderFromReservationCommand, CreateOrderFromReservationResponse>
    {
        private readonly IOrderRepository _orders;
        private readonly IBusPublisher _bus;
        private readonly IHttpClientUtils _httpClient;

        public CreateOrderFromReservationHandler(
            IOrderRepository orders,
            IBusPublisher bus,
            IHttpClientUtils httpClient)
        {
            _orders = orders;
            _bus = bus;
            _httpClient = httpClient;
        }

        public async Task<CreateOrderFromReservationResponse> Handle(
            CreateOrderFromReservationCommand request,
            CancellationToken ct)
        {
            var exists = await _orders.ExistsForReservationAsync(request.ReservationId, ct);
            if (exists)
            {
                return new CreateOrderFromReservationResponse(
                    OrderId: Guid.Empty,
                    UserId: request.UserId,
                    OrderStatus: "AlreadyExists"
                );
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CartId = request.CartId,
                UserId = request.UserId,
                Total = request.Total ?? 0m,
                OrderStatus = "PendingPayment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                CorrelationId = request.CorrelationId,
                ReservationId = request.ReservationId
            };

            await _orders.AddAsync(order, ct);
            await _orders.SaveChangesAsync(ct);
            
            //https://localhost/inventory/
            await _httpClient.SendPostRequest("http://localhost/inventory/api/v1/inventory/cache/apply-order", new { orderId = order.Id });

            return new CreateOrderFromReservationResponse(
                OrderId: order.Id,
                UserId: order.UserId ?? request.UserId,
                OrderStatus: order.OrderStatus
            );
        }
    }
}
