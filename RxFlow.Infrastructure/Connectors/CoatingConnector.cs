using System.Net.Http.Json;
using RxFlow.Application.Abstractions;

namespace RxFlow.Infrastructure.Connectors;

public sealed class CoatingConnector : ICoatingConnector
{
    private readonly HttpClient _httpClient;

    public CoatingConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("coating");
    }

    public async Task<string> GetCoatingRecommendationAsync(Guid orderId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var response = await _httpClient.GetFromJsonAsync<CoatingResponse>($"/coating/{orderId}", cancellationToken);
            if (response is not null)
            {
                return response.Name;
            }
        }

        return "standard";
    }

    private sealed record CoatingResponse(string Name);
}