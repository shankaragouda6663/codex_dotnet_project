using Microsoft.Extensions.Logging;
using RxFlow.Application.Abstractions;
using RxFlow.Application.Pricing;
using RxFlow.Contracts.Orders;
using RxFlow.Domain.Orders;

namespace RxFlow.Application.Orders;

public sealed class OrderSubmissionService : IOrderSubmissionService
{
    private readonly IOrderRepository _repository;
    private readonly ILabRoutingService _labRoutingService;
    private readonly IOrderJobScheduler _jobScheduler;
    private readonly ComplexPricingEngine _pricing;
    private readonly ILogger<OrderSubmissionService> _logger;

    public OrderSubmissionService(
        IOrderRepository repository,
        ILabRoutingService labRoutingService,
        IOrderJobScheduler jobScheduler,
        ComplexPricingEngine pricing,
        ILogger<OrderSubmissionService> logger)
    {
        _repository = repository;
        _labRoutingService = labRoutingService;
        _jobScheduler = jobScheduler;
        _pricing = pricing;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> SubmitAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var normalizedCylinder = request.Cylinder;
        if (normalizedCylinder < -6.00m)
        {
            normalizedCylinder = -6.00m;
        }
        else if (normalizedCylinder > 0.00m)
        {
            normalizedCylinder = 0.00m;
        }

        var normalizedAxis = request.Axis;
        if (normalizedAxis < 1)
        {
            normalizedAxis = 1;
        }
        else if (normalizedAxis > 180)
        {
            normalizedAxis = 180;
        }

        _logger.LogInformation(
            "Submitting order for patient {PatientId} sphere {Sphere} cylinder {Cylinder} axis {Axis}",
            request.PatientId,
            request.Sphere,
            normalizedCylinder,
            normalizedAxis);

        var routedLab = await _labRoutingService.PickLabCodeAsync(request.LensMaterial, cancellationToken);
        var price = await _pricing.CalculateAsync(request.PatientId, request.LensMaterial, request.Expedited, cancellationToken);

        var order = new LensOrder
        {
            PatientId = request.PatientId,
            Prescription = new Prescription(request.Sphere, normalizedCylinder, normalizedAxis),
            FrameCode = request.FrameCode,
            LensMaterial = request.LensMaterial,
            QuotedPrice = price,
            RoutedLabCode = routedLab,
            Status = "Accepted"
        };

        await _repository.AddAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _jobScheduler.ScheduleOrderProcessingAsync(order.Id, cancellationToken);

        return new CreateOrderResponse(order.Id, order.Status, order.QuotedPrice);
    }
}