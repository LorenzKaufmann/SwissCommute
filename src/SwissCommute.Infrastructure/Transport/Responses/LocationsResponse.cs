using SwissCommute.Infrastructure.Transport.Dtos;
using System.Text.Json.Serialization;

namespace SwissCommute.Infrastructure.Transport.Responses;

public sealed record LocationsResponse
{
    [JsonPropertyName("stations")]
    public IReadOnlyList<StationDto> Stations { get; init; } = [];
}

