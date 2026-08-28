namespace RxFlow.Application.Reporting;

public sealed record TimeRangeUtc(DateTimeOffset StartUtc, DateTimeOffset EndUtc);

public sealed record LabThroughputReport(
    int UniqueCompletedJobs,
    int WindowMinutes,
    int OfflineMinutes,
    int OnlineMinutes,
    decimal JobsPerOnlineHour);

public sealed class LabThroughputCalculator
{
    public LabThroughputReport Calculate(
        DateTimeOffset windowStartUtc,
        DateTimeOffset windowEndUtc,
        IReadOnlyCollection<string> completedJobIds,
        IReadOnlyCollection<TimeRangeUtc> offlineIntervalsUtc)
    {
        if (windowEndUtc <= windowStartUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(windowEndUtc), "Reporting window must have positive duration.");
        }

        var uniqueCompletedJobs = completedJobIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .Count();

        var normalizedOfflineIntervals = NormalizeAndMerge(offlineIntervalsUtc
            .Select(x => new TimeRangeUtc(
                x.StartUtc < windowStartUtc ? windowStartUtc : x.StartUtc,
                x.EndUtc > windowEndUtc ? windowEndUtc : x.EndUtc))
            .Where(x => x.EndUtc > x.StartUtc)
            .ToArray());
        var totalWindowMinutes = (int)(windowEndUtc - windowStartUtc).TotalMinutes;
        var offlineMinutes = normalizedOfflineIntervals.Sum(interval => (int)(interval.EndUtc - interval.StartUtc).TotalMinutes);
        var onlineMinutes = Math.Max(0, totalWindowMinutes - offlineMinutes);

        var jobsPerOnlineHour = onlineMinutes == 0
            ? 0m
            : decimal.Round((decimal)uniqueCompletedJobs / (onlineMinutes / 60m), 2, MidpointRounding.AwayFromZero);

        return new LabThroughputReport(
            uniqueCompletedJobs,
            totalWindowMinutes,
            offlineMinutes,
            onlineMinutes,
            jobsPerOnlineHour);
    }

    private static IReadOnlyList<TimeRangeUtc> NormalizeAndMerge(IReadOnlyCollection<TimeRangeUtc> intervalsUtc)
    {
        var validIntervals = intervalsUtc
            .Where(x => x.EndUtc > x.StartUtc)
            .OrderBy(x => x.StartUtc)
            .ToList();

        if (validIntervals.Count == 0)
        {
            return Array.Empty<TimeRangeUtc>();
        }

        var merged = new List<TimeRangeUtc>(validIntervals.Count);
        var current = validIntervals[0];

        for (var i = 1; i < validIntervals.Count; i++)
        {
            var next = validIntervals[i];
            if (next.StartUtc <= current.EndUtc)
            {
                current = current with { EndUtc = next.EndUtc > current.EndUtc ? next.EndUtc : current.EndUtc };
                continue;
            }

            merged.Add(current);
            current = next;
        }

        merged.Add(current);
        return merged;
    }
}
