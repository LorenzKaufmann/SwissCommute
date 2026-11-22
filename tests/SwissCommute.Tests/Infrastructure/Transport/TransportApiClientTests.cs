using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SwissCommute.Core.Transport;
using SwissCommute.Infrastructure.Transport;
using SwissCommute.Tests.Fakes;
using System.Globalization;
using System.Net;

namespace SwissCommute.Tests.Infrastructure.Transport;

public sealed class TransportApiClientTests
{
    private const string BaseUrl = "https://transport.opendata.ch/v1/";
    private static readonly ILogger<TransportApiClient> Logger = NullLogger<TransportApiClient>.Instance;
    
    [Fact]
    public async Task SearchStationsAsync_RecordedResponse_ReturnsStations()
    {
        // Arrange
        var (apiClient, _) = await CreateClientAsync("locations-zuerich-hb.json");

        // Act
        IReadOnlyList<Station> stations = await apiClient.SearchStationsAsync("Zürich HB");

        // Assert
        Assert.Equal(10, stations.Count);
        Station first = stations[0];
        Assert.Equal("8503000", first.Id);
        Assert.Equal("Zürich HB", first.Name);
    }

    [Fact]
    public async Task SearchStationsAsync_QueryWithUmlautAndSpace_EncodesQueryInUrl()
    {
        // Arrange
        var (apiClient, fakeMessageHandler) = await CreateClientAsync("locations-zuerich-hb.json");

        // Act
        await apiClient.SearchStationsAsync("Zürich HB");

        // Assert
        Assert.Equal($"{BaseUrl}locations?query=Z%C3%BCrich%20HB", fakeMessageHandler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetConnectionsAsync_RecordedResponse_ReturnsConnections()
    {
        // Arrange
        int expectedConnectionCount = 4;
        string fromStationId = "8503209";
        string toStationId = "8503000";
        DateTimeOffset requestedDeparture = new(2026, 11, 9, 7, 12, 0, TimeSpan.FromHours(1));
        DateTimeOffset expectedFirstDeparture = new(2026, 11, 9, 7, 19, 0, TimeSpan.FromHours(1));
        var (apiClient, _) = await CreateClientAsync("connections-pfaeffikon-zuerich.json");

        // Act
        IReadOnlyList<Connection> connections = await apiClient.GetConnectionsAsync(fromStationId, toStationId, requestedDeparture);

        // Assert
        Assert.Equal(expectedConnectionCount, connections.Count);
        Connection firstConnection = connections[0];
        Assert.Equal(fromStationId, firstConnection.From.Id);
        Assert.Equal(toStationId, firstConnection.To.Id);
        Assert.Equal(expectedFirstDeparture, firstConnection.PlannedDeparture);
        Assert.Equal(TimeSpan.FromHours(1), firstConnection.PlannedDeparture.Offset);
    }

    [Theory]
    [InlineData("2026-11-09T06:12:00+00:00", "2026-11-09", "07:12")] // winter
    [InlineData("2026-06-09T05:12:00+00:00", "2026-06-09", "07:12")] // summer
    [InlineData("2026-11-09T23:30:00+00:00", "2026-11-10", "00:30")] // midnight
    public async Task GetConnectionsAsync_DepartureInOtherOffset_SendsSwissLocalTime(string departure, string expectedDate, string expectedTime)
    {
        // Arrange
        string fromStationId = "8503209";
        string toStationId = "8503000";
        string expectedUri = $"{BaseUrl}connections?from=8503209&to=8503000&date={expectedDate}&time={expectedTime}";
        DateTimeOffset requestedDeparture = DateTimeOffset.Parse(departure, CultureInfo.InvariantCulture);
        var (apiClient, fakeMessageHandler) = await CreateClientAsync("connections-pfaeffikon-zuerich.json");

        // Act
        await apiClient.GetConnectionsAsync(fromStationId, toStationId, requestedDeparture);

        // Assert
        Assert.Equal(expectedUri, fakeMessageHandler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    private static async Task<(TransportApiClient Client, StubHttpMessageHandler MessageHandler)> CreateClientAsync(string testDataFile)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestData", testDataFile);
        string jsonContent = await File.ReadAllTextAsync(path);
        StubHttpMessageHandler fakeMessageHandler = new(HttpStatusCode.OK, jsonContent);
        HttpClient httpClient = new(fakeMessageHandler) { BaseAddress = new Uri(BaseUrl) };
        TransportApiClient apiClient = new(httpClient, Logger);
        return (apiClient, fakeMessageHandler);
    }
}
