using SwissCommute.Infrastructure.Transport.Dtos;
using SwissCommute.Infrastructure.Transport.Responses;
using System.Net.Http.Json;

namespace SwissCommute.Infrastructure.Transport;

public sealed class TransportApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<StationDto>> SearchStationsAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        string requestUri = $"locations?query={Uri.EscapeDataString(query)}";
        LocationsResponse? response = await httpClient.GetFromJsonAsync<LocationsResponse>(requestUri, cancellationToken);
        return response?.Stations ?? new List<StationDto>();
    }
}
