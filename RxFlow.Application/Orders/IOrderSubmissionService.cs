using RxFlow.Contracts.Orders;

namespace RxFlow.Application.Orders;

public interface IOrderSubmissionService
{
    Task<CreateOrderResponse> SubmitAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}