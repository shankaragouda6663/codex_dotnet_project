using FluentAssertions;
using FsCheck;

namespace RxFlow.Tests.Orders;

public sealed class PropertySmokeTests
{
    [Fact]
    public void FsCheckConfiguration_IsAvailable()
    {
        var config = Config.Quick;
        config.MaxTest.Should().BeGreaterThan(0);
    }
}
