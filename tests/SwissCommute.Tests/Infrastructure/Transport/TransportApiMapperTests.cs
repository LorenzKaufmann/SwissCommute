using SwissCommute.Core.Transport;
using SwissCommute.Infrastructure.Transport;
using SwissCommute.Infrastructure.Transport.Dtos;

namespace SwissCommute.Tests.Infrastructure.Transport;

public sealed class TransportApiMapperTests
{
    private static readonly DateTimeOffset FromPlannedDeparture = new(2026, 11, 23, 7, 19, 0, TimeSpan.FromHours(1));
    private const int FromDelayMinutes = 3;
    private const string FromPlatform = "3";
    private const string FromStationId = "8503209";
    private const string FromStationName = "Pfäffikon SZ";

    private static readonly DateTimeOffset ToPlannedArrival = new(2026, 11, 23, 8, 19, 0, TimeSpan.FromHours(1));
    private const string ToStationId = "8503000";
    private const string ToStationName = "Zürich HB";
    private const string Line = "S5";


    [Fact]
    public void ToStation_ValidStationDto_ReturnsStation()
    {
        // Arrange
        string id = "8503000";
        string name = "Zürich HB";
        StationDto stationDto = new() { Id = id, Name = name };
        
        // Act
        Station? station = TransportApiMapper.ToStation(stationDto);
        
        // Assert
        Assert.NotNull(station);
        Assert.Equal(id, station.Id);
        Assert.Equal(name, station.Name);
    }

    [Theory]
    [InlineData("8503000", null)]
    [InlineData("8503000", "")]
    [InlineData("8503000", " ")]
    [InlineData(null, "Zürich HB")]
    [InlineData("", "Zürich HB")]
    [InlineData(" ", "Zürich HB")]
    public void ToStation_MissingIdOrName_ReturnsNull(string? id, string? name)
    {
        // Arrange
        StationDto stationDto = new() { Id = id, Name = name };
        
        // Act
        Station? station = TransportApiMapper.ToStation(stationDto);
        
        // Assert
        Assert.Null(station);
    }

    [Fact]
    public void ToStation_NullDto_ReturnsNull()
    {
        // Arrange + Act
        Station? station = TransportApiMapper.ToStation(null);

        // Assert
        Assert.Null(station);
    }

    [Fact]
    public void ToConnection_ValidDto_ReturnsConnection()
    {
        // Arrange
        ConnectionDto connectionDto = CreateValidConnectionDto();

        // Act
        Connection? connection = TransportApiMapper.ToConnection(connectionDto);

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(FromStationId, connection.From.Id);
        Assert.Equal(FromStationName, connection.From.Name);
        Assert.Equal(TimeSpan.FromMinutes(FromDelayMinutes), connection.Delay);
        Assert.Equal(FromPlannedDeparture, connection.PlannedDeparture);
        Assert.Equal(FromPlatform, connection.Platform);
        Assert.Equal(ToStationId, connection.To.Id);
        Assert.Equal(ToStationName, connection.To.Name);
        Assert.Equal(ToPlannedArrival, connection.PlannedArrival);
        Assert.Equal(Line, connection.Line);
        Assert.Equal(FromPlannedDeparture + TimeSpan.FromMinutes(FromDelayMinutes), connection.ExpectedDeparture);
    }

    [Theory]
    [InlineData(null, null)]   // no real-time data: must stay null
    [InlineData(0, 0)]         // on time: must become TimeSpan.Zero, NOT null
    [InlineData(3, 3)]         // delayed
    public void ToConnection_DelayInMinutes_MapsToTimeSpan(int? delayMinutes, int? expectedMinutes)
    {
        // Arrange
        ConnectionDto validDto = CreateValidConnectionDto();
        ConnectionDto connectionDto = validDto with { From = validDto.From! with { Delay = delayMinutes } };
        TimeSpan? expectedDelay = expectedMinutes.HasValue ? TimeSpan.FromMinutes(expectedMinutes.Value) : null;

        // Act
        Connection? connection = TransportApiMapper.ToConnection(connectionDto);

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(expectedDelay, connection.Delay);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "S2" }, "S2")]
    [InlineData(new[] { "S2", "S8" }, "S2")]
    public void ToConnection_Products_MapsFirstProductToLine(string[]? products, string? expectedLine)
    {
        // Arrange
        ConnectionDto validDto = CreateValidConnectionDto();
        ConnectionDto connectionDto = validDto with { Products = products };

        // Act
        Connection? connection = TransportApiMapper.ToConnection(connectionDto);

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(expectedLine, connection.Line);
    }

    private static ConnectionDto CreateValidConnectionDto()
    {
        StationDto fromStationDto = new()
        {
            Id = FromStationId,
            Name = FromStationName
        };
        CheckpointDto fromCheckpointDto = new() 
        {
            Delay = FromDelayMinutes,
            Departure = FromPlannedDeparture,
            Platform = FromPlatform,
            Station = fromStationDto
        };

        StationDto toStationDto = new()
        {
            Id = ToStationId,
            Name = ToStationName
        };
        // the destination checkpoint only has arrival data, like in real API responses
        CheckpointDto toCheckpointDto = new()
        {
            Arrival = ToPlannedArrival,
            Station = toStationDto
        };
        return new() { From = fromCheckpointDto, Products = [Line], To = toCheckpointDto };
    }
}
