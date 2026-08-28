using StackExchange.Redis;

namespace RxFlow.Infrastructure.Redis;

public sealed class RedisInFlightCounter : RxFlow.Application.Abstractions.IInFlightCounter
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisInFlightCounter(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task IncrementAsync(string key, CancellationToken cancellationToken)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var current = await db.StringGetAsync(key);
        var parsed = current.HasValue ? int.Parse(current.ToString()) : 0;
        parsed += 1;
        await Task.Run(() => db.StringSet(key, parsed), cancellationToken);
    }

    public async Task DecrementAsync(string key, CancellationToken cancellationToken)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var current = await db.StringGetAsync(key);
        var parsed = current.HasValue ? int.Parse(current.ToString()) : 0;
        parsed -= 1;
        if (parsed < 0)
        {
            parsed = 0;
        }

        await Task.Run(() => db.StringSet(key, parsed), cancellationToken);
    }

    public async Task<int> ReadAsync(string key, CancellationToken cancellationToken)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var current = await db.StringGetAsync(key);
        await Task.CompletedTask;
        return current.HasValue ? int.Parse(current.ToString()) : 0;
    }
}