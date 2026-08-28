using Microsoft.EntityFrameworkCore;
using RxFlow.Application.Abstractions;
using RxFlow.Domain.Orders;

namespace RxFlow.Infrastructure.Persistence;

public sealed class EfOrderRepository : IOrderRepository
{
    private readonly RxFlowDbContext _dbContext;

    public EfOrderRepository(RxFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(LensOrder order, CancellationToken cancellationToken)
    {
        return _dbContext.Orders.AddAsync(order, cancellationToken).AsTask();
    }

    public Task<LensOrder?> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LensOrder>> FindByPatientAsync(string patientId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.Where(x => x.PatientId == patientId).ToListAsync(cancellationToken);
    }
}