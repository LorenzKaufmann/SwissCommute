namespace SwissCommute.Core.Transport;

public sealed record Connection(
    Station From,
    Station To,
    DateTimeOffset PlannedDeparture,
    DateTimeOffset PlannedArrival,
    TimeSpan? Delay,
    string? Platform,
    string? Line)
{
    public DateTimeOffset? ExpectedDeparture => PlannedDeparture + Delay;
}
