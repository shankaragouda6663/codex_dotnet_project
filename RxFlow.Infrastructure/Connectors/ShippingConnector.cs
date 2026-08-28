using System.Net.Http.Json;
using RxFlow.Application.Abstractions;

namespace RxFlow.Infrastructure.Connectors;

public sealed class ShippingConnector : IShippingConnector
{
    private readonly HttpClient _httpClient;

    public ShippingConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("shipping");
    }

    public async Task<string> ReserveShipmentSlotAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("/slots/reserve", new { orderId }, cancellationToken);
        return response.IsSuccessStatusCode ? "reserved" : "pending";
    }
}