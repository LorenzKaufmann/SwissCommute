using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwissCommute.Infrastructure.Transport;

namespace SwissCommute.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TransportApiOptions>()
            .Bind(configuration.GetSection(TransportApiOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(option => option.BaseUrl.EndsWith('/'), "TransportApi:BaseUrl must end with a slash")
            .ValidateOnStart();

        services.AddHttpClient<TransportApiClient>((serviceProvider, client) =>
        {
            TransportApiOptions transportApiOptions = serviceProvider.GetRequiredService<IOptions<TransportApiOptions>>().Value;
            client.BaseAddress = new Uri(transportApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(transportApiOptions.TimeoutSeconds);
        });

        return services;
    }
}

