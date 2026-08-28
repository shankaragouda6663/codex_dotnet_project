namespace RxFlow.Domain.Labs;

public sealed class RxLab
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Capability { get; set; } = string.Empty;
    public int StaticPriority { get; set; }
    public int CurrentQueueDepth { get; set; }
}