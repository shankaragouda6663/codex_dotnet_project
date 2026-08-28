using FluentAssertions;
using Moq;
using RxFlow.Application.Abstractions;
using RxFlow.Application.Pricing;

namespace RxFlow.Tests.Pricing;

public sealed class ComplexPricingEngineTests
{
    [Fact]
    public async Task CalculateAsync_PolyHighBasePrice_AppliesDiscountOnce()
    {
        var insurance = new Mock<IInsuranceConnector>();
        insurance.Setup(x => x.FetchCoverageAdjustmentAsync("PAT-200", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var lensCatalog = new Mock<ILensCatalogConnector>();
        lensCatalog.Setup(x => x.FetchMaterialMultiplierAsync("polycarbonate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(2.0m);

        var sut = new ComplexPricingEngine(insurance.Object, lensCatalog.Object);

        var quoted = await sut.CalculateAsync("PAT-200", "polycarbonate", false, CancellationToken.None);

        quoted.Should().Be(230m);
    }

    [Fact]
    public async Task CalculateAsync_HugeInsuranceAdjustment_RespectsMinimumPriceFloor()
    {
        var insurance = new Mock<IInsuranceConnector>();
        insurance.Setup(x => x.FetchCoverageAdjustmentAsync("PAT-201", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1_000m);

        var lensCatalog = new Mock<ILensCatalogConnector>();
        lensCatalog.Setup(x => x.FetchMaterialMultiplierAsync("standard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1.0m);

        var sut = new ComplexPricingEngine(insurance.Object, lensCatalog.Object);

        var quoted = await sut.CalculateAsync("PAT-201", "standard", false, CancellationToken.None);

        quoted.Should().Be(25m);
    }
}
