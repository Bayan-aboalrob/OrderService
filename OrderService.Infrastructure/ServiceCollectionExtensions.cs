using FlashSaleDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Contracts;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Infrastructure.Services;

namespace OrderService.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
        {
            var sql = cfg.GetConnectionString("FlashSaleDb")
                      ?? "Server=.;Database=FlashSaleDatabase;Trusted_Connection=True;TrustServerCertificate=True";

            services.AddDbContext<FlashSaleDbContext>(opt =>
                opt.UseSqlServer(sql, b => b.MigrationsAssembly(typeof(FlashSaleDbContext).Assembly.FullName)));

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductPricingService, ProductPricingService>();

            services.AddSingleton<IBusPublisher, RabbitMqPublisher>();

            services.AddHostedService<ReservationCartCreatedConsumer>();

            services.AddHostedService<PaymentSucceededSingleConsumer>();


            return services;
        }
    }
}
