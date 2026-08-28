using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RxFlow.Application.Abstractions;
using RxFlow.Application.Orders;
using RxFlow.Application.Pricing;
using RxFlow.Contracts.Orders;
using RxFlow.Domain.Orders;

namespace RxFlow.Tests.Orders;

public sealed class OrderSubmissionServiceTests
{
    [Fact]
    public async Task SubmitAsync_ReturnsAcceptedResponse()
    {
        var repository = new FakeOrderRepository();
        var labRouting = new Mock<ILabRoutingService>();
        labRouting.Setup(x => x.PickLabCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("LAB-A");

        var scheduler = new Mock<IOrderJobScheduler>();
        var insurance = new Mock<IInsuranceConnector>();
        insurance.Setup(x => x.FetchCoverageAdjustmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(0m);

        var catalog = new Mock<ILensCatalogConnector>();
        catalog.Setup(x => x.FetchMaterialMultiplierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(1.2m);

        var service = new OrderSubmissionService(
            repository,
            labRouting.Object,
            scheduler.Object,
            new ComplexPricingEngine(insurance.Object, catalog.Object),
            NullLogger<OrderSubmissionService>.Instance);

        var result = await service.SubmitAsync(
            new CreateOrderRequest("PAT-009", -2.00m, -1.50m, 95, "FRAME-100", "polycarbonate", true),
            CancellationToken.None);

        result.Status.Should().Be("Accepted");
        result.QuotedPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SubmitAsync_SamePayloadSubmittedTwice_CreatesTwoOrders()
    {
        var repository = new FakeOrderRepository();
        var labRouting = new Mock<ILabRoutingService>();
        labRouting.Setup(x => x.PickLabCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("LAB-A");

        var scheduler = new Mock<IOrderJobScheduler>();
        var insurance = new Mock<IInsuranceConnector>();
        insurance.Setup(x => x.FetchCoverageAdjustmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(0m);

        var catalog = new Mock<ILensCatalogConnector>();
        catalog.Setup(x => x.FetchMaterialMultiplierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(1.0m);

        var service = new OrderSubmissionService(
            repository,
            labRouting.Object,
            scheduler.Object,
            new ComplexPricingEngine(insurance.Object, catalog.Object),
            NullLogger<OrderSubmissionService>.Instance);

        var request = new CreateOrderRequest("PAT-100", -1.00m, -1.00m, 90, "FRAME-100", "polycarbonate", false);
        var first = await service.SubmitAsync(request, CancellationToken.None);
        var second = await service.SubmitAsync(request, CancellationToken.None);

        first.OrderId.Should().NotBe(second.OrderId);
    }

    [Fact]
    public async Task SubmitAsync_AxisOutsideRange_IsNormalizedAndStillAccepted()
    {
        var repository = new FakeOrderRepository();
        var labRouting = new Mock<ILabRoutingService>();
        labRouting.Setup(x => x.PickLabCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("LAB-B");

        var scheduler = new Mock<IOrderJobScheduler>();
        var insurance = new Mock<IInsuranceConnector>();
        insurance.Setup(x => x.FetchCoverageAdjustmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(1m);

        var catalog = new Mock<ILensCatalogConnector>();
        catalog.Setup(x => x.FetchMaterialMultiplierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(1.1m);

        var service = new OrderSubmissionService(
            repository,
            labRouting.Object,
            scheduler.Object,
            new ComplexPricingEngine(insurance.Object, catalog.Object),
            NullLogger<OrderSubmissionService>.Instance);

        var result = await service.SubmitAsync(
            new CreateOrderRequest("PAT-099", -2.00m, 7.00m, 500, "FRAME-100", "polycarbonate", false),
            CancellationToken.None);

        result.Status.Should().Be("Accepted");
        repository.Stored.Single().Prescription.Axis.Should().Be(180);
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public List<LensOrder> Stored { get; } = new();

        public Task AddAsync(LensOrder order, CancellationToken cancellationToken)
        {
            Stored.Add(order);
            return Task.CompletedTask;
        }

        public Task<LensOrder?> GetAsync(Guid orderId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Stored.FirstOrDefault(x => x.Id == orderId));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LensOrder>> FindByPatientAsync(string patientId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<LensOrder>>(Stored.Where(x => x.PatientId == patientId).ToList());
        }
    }
}