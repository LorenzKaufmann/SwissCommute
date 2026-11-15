using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwissCommute.Infrastructure;
using SwissCommute.Infrastructure.Transport;

namespace SwissCommute.Tests.Infrastructure;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ValidConfiguration_RegistersConfiguredTransportApiClient() 
    { 
        // Arrange
        IServiceProvider serviceProvider = BuildContainer("https://transport.opendata.ch/v1/", "30");

        // Act
        IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        HttpClient httpClient = httpClientFactory.CreateClient(nameof(TransportApiClient));

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<TransportApiClient>());
        Assert.Equal("https://transport.opendata.ch/v1/", httpClient.BaseAddress?.ToString());
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }


    [Theory]
    [InlineData(null, null, "BaseUrl field is required")]
    [InlineData("", "30", "BaseUrl field is required")]
    [InlineData("invalid-url", "30", "BaseUrl field is not a valid fully-qualified")]
    [InlineData("https://transport.opendata.ch/v1", "30", "BaseUrl must end with a slash")]
    [InlineData("https://transport.opendata.ch/v1/", "0", "TimeoutSeconds must be between 1 and 120")]
    [InlineData("https://transport.opendata.ch/v1/", "121", "TimeoutSeconds must be between 1 and 120")]
    public void AddInfrastructure_InvalidConfiguration_ThrowsOptionsValidationException(string? baseUrl, string? timeoutSeconds, string expectedMessagePart) 
    {
        // Arrange
        IServiceProvider serviceProvider = BuildContainer(baseUrl, timeoutSeconds);
        
        // Act & Assert
        var exception = Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<IOptions<TransportApiOptions>>().Value);
        Assert.Contains(expectedMessagePart, exception.Message);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("invalid-timeoutseconds")]
    [InlineData("")]
    public void AddInfrastructure_InvalidTimeoutSecondsType_ThrowsInvalidOperationException(string? timeoutSeconds)
    {
        // Arrange
        IServiceProvider serviceProvider = BuildContainer("https://transport.opendata.ch/v1/", timeoutSeconds);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IOptions<TransportApiOptions>>().Value);
    }

    private static IServiceProvider BuildContainer(string? baseUrl, string? timeoutSeconds)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TransportApi:BaseUrl"] = baseUrl,
                ["TransportApi:TimeoutSeconds"] = timeoutSeconds
            })
            .Build();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}

