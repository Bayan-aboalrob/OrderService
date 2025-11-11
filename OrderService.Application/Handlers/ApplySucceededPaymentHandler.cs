using System;
using System.Threading;
using System.Threading.Tasks;
using FlashSaleDB;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Commands;
using OrderService.Application.Contracts;

namespace OrderService.Application.Handlers
{
    public sealed class ApplySucceededPaymentHandler
        : IRequestHandler<ApplySucceededPaymentCommand>
    {
        private readonly FlashSaleDbContext _db;
        private readonly IOrderRepository _orders;

        public ApplySucceededPaymentHandler(
            FlashSaleDbContext db,
            IOrderRepository orders)
        {
            _db = db;
            _orders = orders;
        }

        public async Task<Unit> Handle(ApplySucceededPaymentCommand request, CancellationToken ct)
        {
            var payment = await _db.Payment
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, ct);

            if (payment is null)
                return Unit.Value;

            if (!string.Equals(payment.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
                return Unit.Value;

            var order = await _orders.GetByIdAsync(payment.OrderId, ct);
            if (order is null)
                return Unit.Value;

            order.OrderStatus = "Paid";
            order.UpdatedAt = DateTime.UtcNow;

            await _orders.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
