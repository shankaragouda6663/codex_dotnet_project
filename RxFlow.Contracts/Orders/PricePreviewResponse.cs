namespace RxFlow.Contracts.Orders;

public sealed record PricePreviewResponse(decimal QuotedPrice, string RoutedLabCode);
