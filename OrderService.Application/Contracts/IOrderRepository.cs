using FlashSaleDB.Entities;

namespace OrderService.Application.Contracts
{
    public interface IOrderRepository
    {
        Task<bool> ExistsForReservationAsync(Guid reservationId, CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
        Task<bool> ExistsForCartAsync(Guid cartId, CancellationToken ct = default);
        Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
        Task<Order?> GetByReservationIdAsync(Guid reservationId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
