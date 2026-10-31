using System.Text.Json.Serialization;

namespace SwissCommute.Infrastructure.Transport.Dtos;

public sealed record PrognosisDto
{
    [JsonPropertyName("platform")] 
    public string? Platform { get; init; }
    [JsonPropertyName("arrival")] 
    public DateTimeOffset? Arrival { get; init; }
    [JsonPropertyName("departure")] 
    public DateTimeOffset? Departure { get; init; }
}
