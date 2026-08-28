using RxFlow.Application.Abstractions;

namespace RxFlow.Application.Reporting;

public sealed class OrderReportService
{
    private readonly IOrderReportQuery _query;

    public OrderReportService(IOrderReportQuery query)
    {
        _query = query;
    }

    public Task<IReadOnlyList<string>> SearchByPatientFragmentAsync(string fragment, CancellationToken cancellationToken)
    {
        return _query.SearchByPatientAsync(fragment, cancellationToken);
    }
}