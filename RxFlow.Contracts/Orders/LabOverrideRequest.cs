namespace RxFlow.Contracts.Orders;

public sealed record LabOverrideRequest(Guid OrderId, string LabCode, string Reason);