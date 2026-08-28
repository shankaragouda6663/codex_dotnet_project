using FluentAssertions;
using RxFlow.Application.Reporting;

namespace RxFlow.Tests.Reporting;

public sealed class LabThroughputCalculatorTests
{
    [Fact]
    public void Calculate_WhenOfflineIntervalsCrossWindowBoundaries_ComputesUsingClippedOfflineMinutes()
    {
        var sut = new LabThroughputCalculator();
        var windowStart = new DateTimeOffset(2026, 8, 1, 8, 0, 0, TimeSpan.Zero);
        var windowEnd = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
        var completedJobs = Enumerable.Range(1, 60)
            .Select(x => $"JOB-{x:000}")
            .Concat(["JOB-001", "JOB-002"])
            .ToArray();
        var offlineIntervals = new[]
        {
            new TimeRangeUtc(
                new DateTimeOffset(2026, 8, 1, 7, 30, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero)),
            new TimeRangeUtc(
                new DateTimeOffset(2026, 8, 1, 11, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 1, 12, 30, 0, TimeSpan.Zero)),
            new TimeRangeUtc(
                new DateTimeOffset(2026, 8, 1, 11, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 1, 12, 30, 0, TimeSpan.Zero))
        };

        var report = sut.Calculate(windowStart, windowEnd, completedJobs, offlineIntervals);

        report.UniqueCompletedJobs.Should().Be(60);
        report.WindowMinutes.Should().Be(240);
        report.OfflineMinutes.Should().Be(120);
        report.OnlineMinutes.Should().Be(120);
        report.JobsPerOnlineHour.Should().Be(30m);
    }

    [Fact]
    public void Calculate_WhenWindowIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        var sut = new LabThroughputCalculator();
        var windowStart = new DateTimeOffset(2026, 8, 1, 8, 0, 0, TimeSpan.Zero);
        var windowEnd = new DateTimeOffset(2026, 8, 1, 8, 0, 0, TimeSpan.Zero);

        var act = () => sut.Calculate(windowStart, windowEnd, [], []);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
