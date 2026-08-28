using Microsoft.EntityFrameworkCore;
using RxFlow.Application.Labs;

namespace RxFlow.Infrastructure.Labs;

public sealed class EfLabReadStore : ILabReadStore
{
    private readonly Persistence.RxFlowDbContext _dbContext;

    public EfLabReadStore(Persistence.RxFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LabProjection>> GetLabsForCapabilityAsync(string capability, CancellationToken cancellationToken)
    {
        return await _dbContext.Labs
            .Where(x => x.Capability == capability)
            .Select(x => new LabProjection(x.Code, x.StaticPriority, x.CurrentQueueDepth))
            .ToListAsync(cancellationToken);
    }
}