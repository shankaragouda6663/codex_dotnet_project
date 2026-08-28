using System.Text.Json;
using Confluent.Kafka;
using RxFlow.Application.Abstractions;
using RxFlow.Contracts.Orders;

namespace RxFlow.Infrastructure.Messaging;

public sealed class KafkaOrderPublisher : IKafkaOrderPublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaOrderPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAcceptedAsync(OrderAcceptedEvent @event, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(@event);
        await _producer.ProduceAsync(
            "rxflow.orders.accepted",
            new Message<string, string> { Key = @event.OrderId.ToString(), Value = payload },
            cancellationToken);
    }
}