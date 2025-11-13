// Consumer 1
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Application.Orders.Commands;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Infrastructure.Messaging
{
    internal sealed class PaymentSucceededSingleConsumer : BackgroundService
    {
        private readonly ILogger<PaymentSucceededSingleConsumer> _log;
        private readonly IServiceProvider _sp;
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private readonly string _queue;

        public PaymentSucceededSingleConsumer(
            ILogger<PaymentSucceededSingleConsumer> log,
            IServiceProvider sp,
            IConfiguration cfg)
        {
            _log = log;
            _sp = sp;

            var section = cfg.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = section["HostName"] ?? "localhost",
                Port = int.TryParse(section["Port"], out var p) ? p : 5672,
                VirtualHost = section["VirtualHost"] ?? "/",
                UserName = section["UserName"] ?? "guest",
                Password = section["Password"] ?? "guest",
                DispatchConsumersAsync = true
            };

            _conn = factory.CreateConnection("order-payment-consumer");
            _ch = _conn.CreateModel();

            var exchange = section["Exchange"] ?? "flashsale.topic";
            var exchangeType = section["ExchangeType"] ?? "topic";
            _ch.ExchangeDeclare(exchange, exchangeType, durable: true);

            // ✅ Unique queue name for consumer #1
            _queue = "order.payment-succeeded.orderservice.v1";
            _ch.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);

            _ch.QueueBind(_queue, exchange, "Payment.Succeeded");

            _ch.BasicQos(0, 1, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_ch);
            consumer.Received += OnMessageAsync;
            _ch.BasicConsume(_queue, autoAck: false, consumer);
            _log.LogInformation("OrderService is listening to Payment.Succeeded ...");
            return Task.CompletedTask;
        }

        private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<PaymentSucceededIntegrationEvent>(json);

                if (msg is null)
                {
                    _ch.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _sp.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new UpdateOrderStatusCommand(
                    msg.OrderId,
                    "Paid"
                ), CancellationToken.None);

                _ch.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error while handling Payment.Succeeded");
                _ch.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        }

        public override void Dispose()
        {
            _ch?.Close();
            _conn?.Close();
            base.Dispose();
        }
    }
}
