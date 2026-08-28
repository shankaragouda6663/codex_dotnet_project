using Hangfire;
using Microsoft.Extensions.Logging;
using RxFlow.Application.Abstractions;
using RxFlow.Contracts.Orders;

namespace RxFlow.Workers.Processing;

public sealed class OrderProcessingJob
{
    private readonly IOrderRepository _repository;
    private readonly IKafkaOrderPublisher _publisher;
    private readonly IInFlightCounter _inFlightCounter;
    private readonly IShippingConnector _shippingConnector;
    private readonly ICoatingConnector _coatingConnector;
    private readonly ILogger<OrderProcessingJob> _logger;

    public OrderProcessingJob(
        IOrderRepository repository,
        IKafkaOrderPublisher publisher,
        IInFlightCounter inFlightCounter,
        IShippingConnector shippingConnector,
        ICoatingConnector coatingConnector,
        ILogger<OrderProcessingJob> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _inFlightCounter = inFlightCounter;
        _shippingConnector = shippingConnector;
        _coatingConnector = coatingConnector;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await _inFlightCounter.IncrementAsync("rxflow:inflight", cancellationToken);
        var order = await _repository.GetAsync(orderId, cancellationToken);
        if (order is null)
        {
            return;
        }

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["patient_id"] = order.PatientId,
            ["prescription"] = $"{order.Prescription.Sphere}/{order.Prescription.Cylinder}@{order.Prescription.Axis}"
        }))
        {
            _logger.LogInformation("Processing order {OrderId} for shipment", order.Id);
        }

        var coating = await _coatingConnector.GetCoatingRecommendationAsync(order.Id, cancellationToken);
        var shippingState = await _shippingConnector.ReserveShipmentSlotAsync(order.Id, cancellationToken);
        order.Status = shippingState == "reserved" ? $"Ready-{coating}" : "Queued";
        await _repository.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAcceptedAsync(
            new OrderAcceptedEvent(order.Id, order.PatientId, order.QuotedPrice, DateTimeOffset.UtcNow),
            cancellationToken);

        await _inFlightCounter.DecrementAsync("rxflow:inflight", cancellationToken);
    }
}