using System;
using MediatR;

namespace OrderService.Application.Commands
{
    public sealed record ApplySucceededPaymentCommand(Guid PaymentId) : IRequest;
}
