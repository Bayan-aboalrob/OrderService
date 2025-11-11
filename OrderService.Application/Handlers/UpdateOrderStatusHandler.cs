using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OrderService.Application.Commands;
using OrderService.Application.Contracts;

namespace OrderService.Application.Handlers
{
    public sealed class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly IOrderRepository _orders;

        public UpdateOrderStatusHandler(IOrderRepository orders)
        {
            _orders = orders;
        }

        public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
        {
            var order = await _orders.GetByIdAsync(request.OrderId, ct);
            if (order is null)
                return Unit.Value;

            order.OrderStatus = request.NewStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _orders.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
