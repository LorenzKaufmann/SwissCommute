using System.Text.Json.Serialization;

namespace SwissCommute.Infrastructure.Transport.Dtos;

public sealed record CheckpointDto
{
    [JsonPropertyName("station")] 
    public StationDto? Station { get; init; }
    [JsonPropertyName("arrival")] 
    public DateTimeOffset? Arrival { get; init; }
    [JsonPropertyName("departure")] 
    public DateTimeOffset? Departure { get; init; }
    [JsonPropertyName("delay")] 
    public int? Delay { get; init; }
    [JsonPropertyName("platform")] 
    public string? Platform { get; init; }
    [JsonPropertyName("prognosis")] 
    public PrognosisDto? Prognosis { get; init; }
}
