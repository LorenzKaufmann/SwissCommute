namespace SwissCommute.Core.Transport;

public interface ITransportClient
{
    Task<IReadOnlyList<Station>> SearchStationsAsync(string query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Connection>> GetConnectionsAsync(string fromStationId, string toStationId, DateTimeOffset departure, CancellationToken cancellationToken = default);
}
