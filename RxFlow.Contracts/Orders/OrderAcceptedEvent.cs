namespace RxFlow.Contracts.Orders;

public sealed record OrderAcceptedEvent(Guid OrderId, string PatientId, decimal Price, DateTimeOffset AcceptedAtUtc);