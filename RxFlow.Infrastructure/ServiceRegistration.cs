using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RxFlow.Application.Abstractions;
using RxFlow.Application.Labs;
using RxFlow.Application.Orders;
using RxFlow.Application.Pricing;
using RxFlow.Application.Reporting;
using RxFlow.Infrastructure.Connectors;
using RxFlow.Infrastructure.Labs;
using RxFlow.Infrastructure.Messaging;
using RxFlow.Infrastructure.Persistence;
using RxFlow.Infrastructure.Redis;
using RxFlow.Infrastructure.Reporting;
using StackExchange.Redis;

namespace RxFlow.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RxFlowDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
            }).Build());

        services.AddHttpClient("insurance", c => c.BaseAddress = new Uri("http://insurance-gateway"));
        services.AddHttpClient("lens-catalog", c => c.BaseAddress = new Uri("http://lens-catalog"));
        services.AddHttpClient("shipping", c =>
        {
            c.BaseAddress = new Uri("http://shipping-service");
            c.Timeout = TimeSpan.FromSeconds(3);
        });
        services.AddHttpClient("coating", c =>
        {
            c.BaseAddress = new Uri("http://coating-service");
            c.Timeout = TimeSpan.FromSeconds(2);
        });

        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IOrderSubmissionService, OrderSubmissionService>();
        services.AddScoped<ILabReadStore, EfLabReadStore>();
        services.AddScoped<ILabRoutingService, StaticPriorityLabRoutingService>();
        services.AddScoped<IKafkaOrderPublisher, KafkaOrderPublisher>();
        services.AddScoped<IInsuranceConnector, InsuranceConnector>();
        services.AddScoped<ILensCatalogConnector, LensCatalogConnector>();
        services.AddScoped<IShippingConnector, ShippingConnector>();
        services.AddScoped<ICoatingConnector, CoatingConnector>();
        services.AddScoped<IInFlightCounter, RedisInFlightCounter>();
        services.AddScoped<ComplexPricingEngine>();
        services.AddScoped<IOrderReportQuery, RawSqlOrderReportQuery>();
        services.AddScoped<OrderReportService>();

        return services;
    }
}
