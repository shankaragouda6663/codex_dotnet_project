using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RxFlow.Infrastructure.Persistence;
using RxFlow.Infrastructure.Reporting;

namespace RxFlow.Tests.Infrastructure;

public sealed class RawSqlOrderReportQueryTests
{
    [Fact(Skip = "Requires local PostgreSQL")]
    public async Task SearchByPatientAsync_WithSimpleInput_ReturnsList()
    {
        var options = new DbContextOptionsBuilder<RxFlowDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=rxflow;Username=rxflow;Password=rxflow")
            .Options;

        await using var db = new RxFlowDbContext(options);
        var sut = new RawSqlOrderReportQuery(db);

        Func<Task> act = async () => await sut.SearchByPatientAsync("PAT", CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
