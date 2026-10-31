using System.Text.Json.Serialization;

namespace SwissCommute.Infrastructure.Transport.Dtos;

public sealed record ConnectionDto
{
    [JsonPropertyName("from")] 
    public CheckpointDto? From { get; init; }
    [JsonPropertyName("to")] 
    public CheckpointDto? To { get; init; }
    [JsonPropertyName("duration")] 
    public string? Duration { get; init; }
    [JsonPropertyName("products")] 
    public IReadOnlyList<string>? Products { get; init; }
}
