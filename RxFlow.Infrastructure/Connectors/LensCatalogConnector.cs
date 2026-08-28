using System.Net.Http.Json;
using RxFlow.Application.Abstractions;

namespace RxFlow.Infrastructure.Connectors;

public sealed class LensCatalogConnector : ILensCatalogConnector
{
    private readonly HttpClient _httpClient;

    public LensCatalogConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("lens-catalog");
    }

    public async Task<decimal> FetchMaterialMultiplierAsync(string lensMaterial, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<MultiplierResponse>($"/materials/{lensMaterial}", cancellationToken);
        return response?.Multiplier ?? 1.0m;
    }

    private sealed record MultiplierResponse(decimal Multiplier);
}