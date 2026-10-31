using System.Text.Json.Serialization;

namespace SwissCommute.Infrastructure.Transport.Dtos;

public sealed record StationDto
{
    [JsonPropertyName("id")] 
    public string? Id { get; init; }
    [JsonPropertyName("name")] 
    public string? Name { get; init; }
}

