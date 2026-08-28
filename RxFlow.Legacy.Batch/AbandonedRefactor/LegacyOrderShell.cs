namespace RxFlow.Legacy.Batch.AbandonedRefactor;

public sealed class LegacyOrderShell
{
    public string order_status { get; set; } = "new";
    public string PatientIdentifier { get; set; } = string.Empty;
}