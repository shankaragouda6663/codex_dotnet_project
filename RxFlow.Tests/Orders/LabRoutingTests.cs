using FluentAssertions;
using Moq;
using RxFlow.Application.Labs;

namespace RxFlow.Tests.Orders;

public sealed class LabRoutingTests
{
    [Fact]
    public async Task PickLabCodeAsync_UsesStaticPriority()
    {
        var store = new Mock<ILabReadStore>();
        store.Setup(x => x.GetLabsForCapabilityAsync("polycarbonate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LabProjection>
            {
                new("LAB-A", 1, 9),
                new("LAB-B", 2, 0)
            });

        var sut = new StaticPriorityLabRoutingService(store.Object);
        var labCode = await sut.PickLabCodeAsync("polycarbonate", CancellationToken.None);

        labCode.Should().Be("LAB-A");
    }
}