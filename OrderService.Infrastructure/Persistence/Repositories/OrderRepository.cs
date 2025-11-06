using FlashSaleDB;
using FlashSaleDB.Entities;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Contracts;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    internal sealed class OrderRepository : IOrderRepository
    {
        private readonly FlashSaleDbContext _db;
        public OrderRepository(FlashSaleDbContext db) => _db = db;

        private DbSet<Order> Orders => _db.Set<Order>();

        public Task<bool> ExistsForReservationAsync(Guid reservationId, CancellationToken ct = default)
            => Orders.AnyAsync(o => o.ReservationId == reservationId, ct);

        public Task AddAsync(Order order, CancellationToken ct = default)
            => Orders.AddAsync(order, ct).AsTask();

        public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
            => Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
