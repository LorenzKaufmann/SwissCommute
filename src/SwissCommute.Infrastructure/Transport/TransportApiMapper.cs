using SwissCommute.Core.Transport;
using SwissCommute.Infrastructure.Transport.Dtos;

namespace SwissCommute.Infrastructure.Transport;

public static class TransportApiMapper
{
    public static Station? ToStation(StationDto? stationDto)
    {
        if (stationDto == null || string.IsNullOrWhiteSpace(stationDto.Id) || string.IsNullOrWhiteSpace(stationDto.Name))
        {
            return null;
        }
        return new Station(stationDto.Id, stationDto.Name);
    }

    public static Connection? ToConnection(ConnectionDto? connectionDto)
    {
        if (connectionDto == null)
        {
            return null;
        }

        CheckpointDto? toCheckpointDto = connectionDto.To;
        CheckpointDto? fromCheckpointDto = connectionDto.From;

        if (fromCheckpointDto == null ||
            fromCheckpointDto.Departure == null ||
            toCheckpointDto == null ||
            toCheckpointDto.Arrival == null
            )
        {
            return null;
        }

        Station? toStation = ToStation(toCheckpointDto.Station);
        Station? fromStation = ToStation(fromCheckpointDto.Station);

        if (toStation == null || fromStation == null)
        {
            return null;
        }

        TimeSpan? delayAsTimeSpan = fromCheckpointDto.Delay is int minutes ? TimeSpan.FromMinutes(minutes) : null;

        return new Connection(
            fromStation,
            toStation,
            fromCheckpointDto.Departure.Value,
            toCheckpointDto.Arrival.Value,
            delayAsTimeSpan,
            fromCheckpointDto.Platform,
            connectionDto.Products is [var line, ..] ? line : null
            );
    }
}
