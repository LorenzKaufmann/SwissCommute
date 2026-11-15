using System.ComponentModel.DataAnnotations;

namespace SwissCommute.Infrastructure.Transport;

public sealed class TransportApiOptions
{
    public const string SectionName = "TransportApi";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;

    [Range(1, 120)]
    public int TimeoutSeconds { get; init; }
}

