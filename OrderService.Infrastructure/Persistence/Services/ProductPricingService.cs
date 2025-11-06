using FlashSaleDB;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Contracts;

namespace OrderService.Infrastructure.Services
{
    internal sealed class ProductPricingService : IProductPricingService
    {
        private readonly FlashSaleDbContext _db;
        public ProductPricingService(FlashSaleDbContext db) => _db = db;

        public async Task<(decimal price, decimal discount)> GetPriceAsync(Guid productId, CancellationToken ct = default)
        {
            var product = await _db.Product
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(p => p.Id == productId, ct);

            if (product == null)
                return (0m, 0m);

            var discount = product.Sale != null ? product.Sale.Discount : 0m;
            return (product.Price, discount);
        }
    }
}
