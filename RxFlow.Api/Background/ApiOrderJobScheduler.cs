using Hangfire;
using RxFlow.Application.Abstractions;
using RxFlow.Workers.Processing;

namespace RxFlow.Api.Background;

public sealed class ApiOrderJobScheduler : IOrderJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ApiOrderJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task ScheduleOrderProcessingAsync(Guid orderId, CancellationToken cancellationToken)
    {
        _backgroundJobClient.Enqueue<OrderProcessingJob>(x => x.ProcessAsync(orderId, CancellationToken.None));
        return Task.CompletedTask;
    }
}