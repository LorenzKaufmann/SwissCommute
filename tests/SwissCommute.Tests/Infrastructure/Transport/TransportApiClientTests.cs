using SwissCommute.Infrastructure.Transport;
using SwissCommute.Infrastructure.Transport.Dtos;
using SwissCommute.Tests.Fakes;
using System.Net;

namespace SwissCommute.Tests.Infrastructure.Transport;

public sealed class TransportApiClientTests
{
    [Fact]
    public async Task SearchStationsAsync_RecordedResponse_ReturnsStations()
    {
        // Arrange
        string path = Path.Combine(AppContext.BaseDirectory, "TestData", "locations-zuerich-hb.json");
        string jsonContent = await File.ReadAllTextAsync(path);

        StubHttpMessageHandler fakeMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, jsonContent);
        HttpClient httpClient = new HttpClient(fakeMessageHandler) { BaseAddress = new Uri("https://transport.opendata.ch/v1") };
        TransportApiClient apiClient = new TransportApiClient(httpClient);

        // Act
        IReadOnlyList<StationDto> stations = await apiClient.SearchStationsAsync("Zürich HB");

        // Assert
        Assert.Equal(10, stations.Count);
        StationDto first = stations.First();
        Assert.Equal("8503000", first.Id);
        Assert.Equal("Zürich HB", first.Name);
    }
}
