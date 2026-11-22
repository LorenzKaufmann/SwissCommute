using SwissCommute.Core.Transport;
using SwissCommute.Infrastructure.Transport;
using SwissCommute.Infrastructure.Transport.Dtos;

namespace SwissCommute.Tests.Infrastructure.Transport;

public sealed class TransportApiMapperTests
{
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
}
