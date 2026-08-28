namespace RxFlow.Contracts.Orders;

public sealed record CreateOrderResponse(Guid OrderId, string Status, decimal QuotedPrice);