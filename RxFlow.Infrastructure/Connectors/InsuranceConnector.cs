using System.Net.Http.Json;
using RxFlow.Application.Abstractions;

namespace RxFlow.Infrastructure.Connectors;

public sealed class InsuranceConnector : IInsuranceConnector
{
    private readonly HttpClient _httpClient;

    public InsuranceConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("insurance");
    }

    public async Task<decimal> FetchCoverageAdjustmentAsync(string patientId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<InsuranceResponse>($"/coverage/{patientId}", cancellationToken);
                return response?.Adjustment ?? 0m;
            }
            catch when (attempt < 2)
            {
            }
        }

        return 0m;
    }

    private sealed record InsuranceResponse(decimal Adjustment);
}