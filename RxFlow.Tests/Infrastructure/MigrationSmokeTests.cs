using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RxFlow.Infrastructure.Persistence;

namespace RxFlow.Tests.Infrastructure;

public sealed class MigrationSmokeTests
{
    [Fact(Skip = "Requires local PostgreSQL")]
    public void MigrationList_ShouldContainExpectedEntries()
    {
        var options = new DbContextOptionsBuilder<RxFlowDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=rxflow;Username=rxflow;Password=rxflow")
            .Options;

        using var db = new RxFlowDbContext(options);
        var migrations = db.Database.GetMigrations().ToList();
        migrations.Should().Contain(x => x.Contains("M0003_AddOrderStatusIndex"));
    }
}
