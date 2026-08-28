namespace RxFlow.Domain.Orders;

public sealed class LensOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PatientId { get; set; } = string.Empty;
    public Prescription Prescription { get; set; } = new(0, 0, 90);
    public string FrameCode { get; set; } = string.Empty;
    public string LensMaterial { get; set; } = string.Empty;
    public string? RoutedLabCode { get; set; }
    public decimal QuotedPrice { get; set; }
    public string Status { get; set; } = "Accepted";
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}