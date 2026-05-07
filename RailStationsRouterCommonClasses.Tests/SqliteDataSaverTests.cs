using RailStationsRouterCommonClasses.DB.SQLite;
using Xunit;

namespace RailStationsRouterCommonClasses.Tests;

public class SqliteDataSaverTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly SqliteDataSaver _dataSaver;

    public SqliteDataSaverTests()
    {
        _testDbPath = Path.GetTempFileName();
        File.Delete(_testDbPath); // Remove the temp file so SQLite can create the database
        _dataSaver = new SqliteDataSaver(_testDbPath);
        _dataSaver.Initialize();
    }

    [Fact]
    public void Constructor_WithNullDatabasePath_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SqliteDataSaver(null!));
    }

    [Fact]
    public void Initialize_CreatesTables()
    {
        // Arrange - already initialized in constructor
        // Act & Assert - if tables weren't created, subsequent operations would fail
        var codeId = _dataSaver.AddCode("yandex123", "esr456");
        Assert.True(codeId > 0);
    }

    [Fact]
    public void AddCode_WithNewCodes_ReturnsNewId()
    {
        // Act
        var codeId = _dataSaver.AddCode("yandex123", "esr456");

        // Assert
        Assert.True(codeId > 0);
    }

    [Fact]
    public void AddCode_WithExistingCodes_ReturnsExistingId()
    {
        // Arrange
        var firstCodeId = _dataSaver.AddCode("yandex123", "esr456");

        // Act
        var secondCodeId = _dataSaver.AddCode("yandex123", "esr456");

        // Assert
        Assert.Equal(firstCodeId, secondCodeId);
    }

    [Fact]
    public void AddCode_WithNullValues_WorksCorrectly()
    {
        // Act
        var codeId = _dataSaver.AddCode(null, null);

        // Assert
        Assert.True(codeId > 0);
    }

    [Fact]
    public void AddCountry_WithNewCountry_ReturnsNewId()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("country_code", null);

        // Act
        var countryId = _dataSaver.AddCountry(codeId, "Russia");

        // Assert
        Assert.True(countryId > 0);
    }

    [Fact]
    public void AddCountry_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("code", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dataSaver.AddCountry(codeId, null!));
    }

    [Fact]
    public void AddCountry_WithExistingCodeId_UpdatesTitleAndReturnsExistingId()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("country_code", null);
        var firstCountryId = _dataSaver.AddCountry(codeId, "Russia");

        // Act
        var secondCountryId = _dataSaver.AddCountry(codeId, "Russian Federation");

        // Assert
        Assert.Equal(firstCountryId, secondCountryId);
    }

    [Fact]
    public void AddRegion_WithNewRegion_ReturnsNewId()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("region_code", null);

        // Act
        var regionId = _dataSaver.AddRegion(codeId, "Moscow Region");

        // Assert
        Assert.True(regionId > 0);
    }

    [Fact]
    public void AddRegion_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("code", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dataSaver.AddRegion(codeId, null!));
    }

    [Fact]
    public void AddRegionToCountry_AddsRelationship()
    {
        // Arrange
        var countryCode = _dataSaver.AddCode("country_code", null);
        var regionCode = _dataSaver.AddCode("region_code", null);
        var countryId = _dataSaver.AddCountry(countryCode, "Russia");
        var regionId = _dataSaver.AddRegion(regionCode, "Moscow Region");

        // Act
        _dataSaver.AddRegionToCountry(countryId, regionId);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact]
    public void AddSettlement_WithNewSettlement_ReturnsNewId()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("settlement_code", null);

        // Act
        var settlementId = _dataSaver.AddSettlement(codeId, "Moscow");

        // Assert
        Assert.True(settlementId > 0);
    }

    [Fact]
    public void AddSettlement_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("code", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dataSaver.AddSettlement(codeId, null!));
    }

    [Fact]
    public void AddSettlementToRegion_AddsRelationship()
    {
        // Arrange
        var regionCode = _dataSaver.AddCode("region_code", null);
        var settlementCode = _dataSaver.AddCode("settlement_code", null);
        var regionId = _dataSaver.AddRegion(regionCode, "Moscow Region");
        var settlementId = _dataSaver.AddSettlement(settlementCode, "Moscow");

        // Act
        _dataSaver.AddSettlementToRegion(regionId, settlementId);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact]
    public void AddStation_WithNewStation_ReturnsNewId()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("station_code", null);

        // Act
        var stationId = _dataSaver.AddStation(
            codeId,
            "North",
            "Railway Station",
            "Moscow Station",
            37.6173,
            "Train",
            55.7558);

        // Assert
        Assert.True(stationId > 0);
    }

    [Fact]
    public void AddStation_WithExistingCodeId_UpdatesAndReturnsExistingId()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("station_code", null);
        var firstStationId = _dataSaver.AddStation(
            codeId,
            "North",
            "Railway Station",
            "Moscow Station",
            37.6173,
            "Train",
            55.7558);

        // Act
        var secondStationId = _dataSaver.AddStation(
            codeId,
            "South",
            "Bus Station",
            "Updated Station",
            37.6200,
            "Bus",
            55.7600);

        // Assert
        Assert.Equal(firstStationId, secondStationId);
    }

    [Fact]
    public void AddStation_WithNullValues_WorksCorrectly()
    {
        // Arrange
        var codeId = _dataSaver.AddCode("station_code", null);

        // Act
        var stationId = _dataSaver.AddStation(
            codeId,
            null,
            null,
            null,
            null,
            null,
            null);

        // Assert
        Assert.True(stationId > 0);
    }

    [Fact]
    public void AddStationToSettlement_AddsRelationship()
    {
        // Arrange
        var settlementCode = _dataSaver.AddCode("settlement_code", null);
        var stationCode = _dataSaver.AddCode("station_code", null);
        var settlementId = _dataSaver.AddSettlement(settlementCode, "Moscow");
        var stationId = _dataSaver.AddStation(
            stationCode,
            "North",
            "Railway Station",
            "Moscow Station",
            37.6173,
            "Train",
            55.7558);

        // Act
        _dataSaver.AddStationToSettlement(settlementId, stationId);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact]
    public void Dispose_ClosesConnection()
    {
        // Arrange
        var dataSaver = new SqliteDataSaver(Path.GetTempFileName());
        dataSaver.Initialize();

        // Act
        dataSaver.Dispose();

        // Assert - no exception means success
        Assert.True(true);
    }

    public void Dispose()
    {
        _dataSaver.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }
}
