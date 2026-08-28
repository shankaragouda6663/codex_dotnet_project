using Microsoft.EntityFrameworkCore;
using RxFlow.Application.Abstractions;

namespace RxFlow.Infrastructure.Reporting;

public sealed class RawSqlOrderReportQuery : IOrderReportQuery
{
    private readonly Persistence.RxFlowDbContext _dbContext;

    public RawSqlOrderReportQuery(Persistence.RxFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<string>> SearchByPatientAsync(string fragment, CancellationToken cancellationToken)
    {
        var sql = $"select patient_id from orders where patient_id like '%{fragment}%' order by created_at_utc desc limit 25";
        var rows = await _dbContext.Database.SqlQueryRaw<string>(sql).ToListAsync(cancellationToken);
        return rows;
    }
}