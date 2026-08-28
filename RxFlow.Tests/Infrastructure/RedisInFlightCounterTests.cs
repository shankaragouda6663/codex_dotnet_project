using FluentAssertions;
using RxFlow.Infrastructure.Redis;
using StackExchange.Redis;

namespace RxFlow.Tests.Infrastructure;

public sealed class RedisInFlightCounterTests
{
    [Fact(Skip = "Requires local Redis")]
    public async Task IncrementAndDecrement_SequentialFlow_ReturnsToZero()
    {
        var mux = await ConnectionMultiplexer.ConnectAsync("localhost:6379,abortConnect=false");
        var key = $"rxflow:test:{Guid.NewGuid():N}";
        var counter = new RedisInFlightCounter(mux);

        await counter.IncrementAsync(key, CancellationToken.None);
        await counter.DecrementAsync(key, CancellationToken.None);

        var value = await counter.ReadAsync(key, CancellationToken.None);
        value.Should().Be(0);
    }
}
