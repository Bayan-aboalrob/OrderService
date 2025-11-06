using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Contracts;

namespace OrderService.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orders;
        public OrdersController(IOrderRepository orders) => _orders = orders;

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var order = await _orders.GetByIdAsync(id, ct);
            return order is null ? NotFound() : Ok(order);
        }
    }
}
