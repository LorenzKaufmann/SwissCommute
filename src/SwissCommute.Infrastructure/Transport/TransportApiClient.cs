using Microsoft.Extensions.Logging;
using SwissCommute.Core.Transport;
using SwissCommute.Infrastructure.Transport.Dtos;
using SwissCommute.Infrastructure.Transport.Responses;
using System.Globalization;
using System.Net.Http.Json;

namespace SwissCommute.Infrastructure.Transport;

internal sealed class TransportApiClient(HttpClient httpClient, ILogger<TransportApiClient> logger) : ITransportClient
{
    private static readonly TimeZoneInfo SwissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

    public async Task<IReadOnlyList<Station>> SearchStationsAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        string requestUri = $"locations?query={Uri.EscapeDataString(query)}";
        LocationsResponse? response = await httpClient.GetFromJsonAsync<LocationsResponse>(requestUri, cancellationToken);

        IReadOnlyList<StationDto> stationDtos = response?.Stations ?? [];

        List<Station> stations = [];

        foreach (var stationDto in stationDtos)
        {
            Station? station = TransportApiMapper.ToStation(stationDto);
            if (station == null)
            {
                logger.LogWarning(
                    "Skipped a station with query {Query} because required data was missing.",
                    query
                    );
                continue;
            }
            stations.Add(station);
        }

        return stations;
    }

    public async Task<IReadOnlyList<Connection>> GetConnectionsAsync(string fromStationId, string toStationId, DateTimeOffset departure, CancellationToken cancellationToken = default)
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

        IReadOnlyList<ConnectionDto> connectionDtos = response?.Connections ?? [];

        List<Connection> connections = [];

        foreach (var connectionDto in connectionDtos)
        {
            Connection? connection = TransportApiMapper.ToConnection(connectionDto);
            if (connection == null)
            {
                logger.LogWarning(
                    "Skipped a connection from {FromStationId} to {ToStationId} because required data was missing.",
                    fromStationId,
                    toStationId
                    );
                continue;
            }
            connections.Add(connection);
        }

        return connections;
    }
}
