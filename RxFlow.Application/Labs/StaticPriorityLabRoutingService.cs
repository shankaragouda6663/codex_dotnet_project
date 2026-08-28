using RxFlow.Application.Abstractions;

namespace RxFlow.Application.Labs;

public sealed class StaticPriorityLabRoutingService : ILabRoutingService
{
    private readonly ILabReadStore _labReadStore;

    public StaticPriorityLabRoutingService(ILabReadStore labReadStore)
    {
        _labReadStore = labReadStore;
    }

    public async Task<string> PickLabCodeAsync(string lensMaterial, CancellationToken cancellationToken)
    {
        var labs = await _labReadStore.GetLabsForCapabilityAsync(lensMaterial, cancellationToken);
        var selected = labs
            .OrderBy(x => x.StaticPriority)
            .FirstOrDefault();

        return selected?.Code ?? "LAB-FALLBACK";
    }
}

public interface ILabReadStore
{
    Task<IReadOnlyList<LabProjection>> GetLabsForCapabilityAsync(string capability, CancellationToken cancellationToken);
}

public sealed record LabProjection(string Code, int StaticPriority, int LiveInFlight);