using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Contracts;
using MediatR;
using OrderService.Application.Dtos;
using OrderService.Application.Orders.Commands;

namespace OrderService.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orders;
        private readonly IMediator _mediator;

        public OrdersController(IOrderRepository orders, IMediator mediator)
        {
            _orders = orders;
            _mediator = mediator;
        }

        // GET /api/v1/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var order = await _orders.GetByIdAsync(id, ct);
            return order is null ? NotFound() : Ok(order);
        }

        // POST /api/v1/orders/from-reservation
        // this is the sync endpoint the it will be called right after creating a reservation(I added for the sync approach)
        [HttpPost("from-reservation")]
        public async Task<ActionResult<CreateOrderFromReservationResponse>> CreateFromReservation(
            [FromBody] CreateOrderFromReservationRequest req,
            CancellationToken ct)
        {
            var cmd = new CreateOrderFromReservationCommand(
                req.UserId,
                req.CartId,
                req.ReservationId,
                req.CorrelationId,
                req.Total
            );

            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
    }
}
