using SwissCommute.Infrastructure.Transport.Dtos;
using System.Text.Json.Serialization;

namespace SwissCommute.Infrastructure.Transport.Responses;

public sealed record ConnectionsResponse
{
    [JsonPropertyName("connections")] 
    public IReadOnlyList<ConnectionDto> Connections { get; init; } = [];
}

