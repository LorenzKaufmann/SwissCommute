using SwissCommute.Infrastructure.Transport;
using SwissCommute.Infrastructure.Transport.Dtos;
using SwissCommute.Tests.Fakes;
using System.Net;

namespace SwissCommute.Tests.Infrastructure.Transport;

public sealed class TransportApiClientTests
{
    private const string BaseUrl = "https://transport.opendata.ch/v1/";

    [Fact]
    public async Task SearchStationsAsync_RecordedResponse_ReturnsStations()
    {
        // Arrange
        var (apiClient, _) = await CreateClientAsync("locations-zuerich-hb.json");

        // Act
        IReadOnlyList<StationDto> stations = await apiClient.SearchStationsAsync("Zürich HB");

        // Assert
        Assert.Equal(10, stations.Count);
        StationDto first = stations.First();
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

    private static async Task<(TransportApiClient Client, StubHttpMessageHandler MessageHandler)> CreateClientAsync(string testDataFile)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestData", testDataFile);
        string jsonContent = await File.ReadAllTextAsync(path);
        StubHttpMessageHandler fakeMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, jsonContent);
        HttpClient httpClient = new HttpClient(fakeMessageHandler) { BaseAddress = new Uri(BaseUrl) };
        TransportApiClient apiClient = new TransportApiClient(httpClient);
        return (apiClient, fakeMessageHandler);
    }
}
