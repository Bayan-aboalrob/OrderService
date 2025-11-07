using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Dtos;
using OrderService.Application.Orders.Commands;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Infrastructure.Messaging
{
    internal sealed class ReservationCreatedParallelConsumer : BackgroundService
    {
        private readonly ILogger<ReservationCreatedParallelConsumer> _log;
        private readonly IServiceProvider _sp;
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private readonly string _queue;
        private readonly SemaphoreSlim _concurrency;

        public ReservationCreatedParallelConsumer(
            ILogger<ReservationCreatedParallelConsumer> log,
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

            _conn = factory.CreateConnection("order-consumer-parallel");
            _ch = _conn.CreateModel();

            var exchange = section["Exchange"] ?? "flashsale.topic";
            var exchangeType = section["ExchangeType"] ?? "topic";

            _ch.ExchangeDeclare(exchange, exchangeType, durable: true);

            _queue = "order.reservation-created.v1";
            _ch.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);
            _ch.QueueBind(_queue, exchange, "Reservation.Created");

            var maxParallel = cfg.GetValue<int?>("ReservationConsumer:Concurrency") ?? 16;
            _concurrency = new SemaphoreSlim(maxParallel, maxParallel);

            _ch.BasicQos(0, (ushort)maxParallel, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_ch);
            consumer.Received += OnMessageAsync;
            _ch.BasicConsume(_queue, autoAck: false, consumer);
            _log.LogInformation("OrderService parallel consumer started (multi-message)...");
            return Task.CompletedTask;
        }

        private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            await _concurrency.WaitAsync();

            _ = ProcessMessageAsync(ea); 
        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<ReservationCreatedIntegrationEvent>(json);

                if (msg == null)
                {
                    _ch.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _sp.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new CreateOrderFromReservationCommand(
                    msg.Id,
                    msg.UserId,
                    msg.ProductId,
                    msg.Quantity,
                    msg.ExpiryTimeUtc,
                    msg.CorrelationId
                ));

                _ch.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error while handling Reservation.Created (parallel)");
                _ch.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
            finally
            {
                _concurrency.Release();
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
