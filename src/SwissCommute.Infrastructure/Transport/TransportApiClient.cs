using SwissCommute.Infrastructure.Transport.Dtos;
using SwissCommute.Infrastructure.Transport.Responses;
using System.Globalization;
using System.Net.Http.Json;

namespace SwissCommute.Infrastructure.Transport;

public sealed class TransportApiClient(HttpClient httpClient)
{
    private static readonly TimeZoneInfo SwissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

    public async Task<IReadOnlyList<StationDto>> SearchStationsAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        string requestUri = $"locations?query={Uri.EscapeDataString(query)}";
        LocationsResponse? response = await httpClient.GetFromJsonAsync<LocationsResponse>(requestUri, cancellationToken);
        return response?.Stations ?? new List<StationDto>();
    }

    public async Task<IReadOnlyList<ConnectionDto>> GetConnectionsAsync(string fromStationId, string toStationId, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromStationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toStationId);

        DateTimeOffset swissDeparture = TimeZoneInfo.ConvertTime(departure, SwissTimeZone);
        string escapedFromStationId = Uri.EscapeDataString(fromStationId);
        string escapedToStationId = Uri.EscapeDataString(toStationId);
        string swissDepartureDate = swissDeparture.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string swissDepartureTime = swissDeparture.ToString("HH:mm", CultureInfo.InvariantCulture);
        string requestUri = $"connections?from={escapedFromStationId}&to={escapedToStationId}&date={swissDepartureDate}&time={swissDepartureTime}";
        ConnectionsResponse? response = await httpClient.GetFromJsonAsync<ConnectionsResponse>(requestUri, cancellationToken);
        return response?.Connections ?? new List<ConnectionDto>();
    }
}
