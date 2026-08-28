using RxFlow.Contracts.Orders;
using RxFlow.Domain.Orders;

namespace RxFlow.Application.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(LensOrder order, CancellationToken cancellationToken);
    Task<LensOrder?> GetAsync(Guid orderId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<LensOrder>> FindByPatientAsync(string patientId, CancellationToken cancellationToken);
}

public interface ILabRoutingService
{
    Task<string> PickLabCodeAsync(string lensMaterial, CancellationToken cancellationToken);
}

public interface IOrderJobScheduler
{
    Task ScheduleOrderProcessingAsync(Guid orderId, CancellationToken cancellationToken);
}

public interface IKafkaOrderPublisher
{
    Task PublishAcceptedAsync(OrderAcceptedEvent @event, CancellationToken cancellationToken);
}

public interface IInsuranceConnector
{
    Task<decimal> FetchCoverageAdjustmentAsync(string patientId, CancellationToken cancellationToken);
}

public interface ILensCatalogConnector
{
    Task<decimal> FetchMaterialMultiplierAsync(string lensMaterial, CancellationToken cancellationToken);
}

public interface IShippingConnector
{
    Task<string> ReserveShipmentSlotAsync(Guid orderId, CancellationToken cancellationToken);
}

public interface ICoatingConnector
{
    Task<string> GetCoatingRecommendationAsync(Guid orderId, CancellationToken cancellationToken);
}

public interface IInFlightCounter
{
    Task IncrementAsync(string key, CancellationToken cancellationToken);
    Task DecrementAsync(string key, CancellationToken cancellationToken);
    Task<int> ReadAsync(string key, CancellationToken cancellationToken);
}

public interface IOrderReportQuery
{
    Task<IReadOnlyList<string>> SearchByPatientAsync(string fragment, CancellationToken cancellationToken);
}