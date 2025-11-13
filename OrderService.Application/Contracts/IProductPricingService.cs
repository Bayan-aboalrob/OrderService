namespace OrderService.Application.Contracts
{
    public interface IProductPricingService
    {
        Task<(decimal price, decimal discount)> GetPriceAsync(Guid productId, CancellationToken ct = default);
    }
}
