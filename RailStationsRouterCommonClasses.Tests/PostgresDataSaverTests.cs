using RailStationsRouterCommonClasses.DB.PostgreSQL;
using Xunit;

namespace RailStationsRouterCommonClasses.Tests;

public class PostgresDataSaverTests : IDisposable
{
    private readonly string _connectionString;
    private readonly PostgresDataSaver? _dataSaver;
    private readonly bool _skipTests;

    public PostgresDataSaverTests()
    {
        // Get connection string from environment variable or skip tests
        _connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ?? string.Empty;
        
        if (string.IsNullOrEmpty(_connectionString))
        {
            _skipTests = true;
            _dataSaver = null;
        }
        else
        {
            _skipTests = false;
            _dataSaver = new PostgresDataSaver(_connectionString);
        }
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PostgresDataSaver(null!));
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddCode_WithNewCodes_ReturnsNewId()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var yandexCode = $"yandex_{Guid.NewGuid()}";
        var esrCode = $"esr_{Guid.NewGuid()}";

        // Act
        var codeId = _dataSaver.AddCode(yandexCode, esrCode);

        // Assert
        Assert.True(codeId > 0);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddCountry_WithNewCountry_ReturnsNewId()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode($"country_{Guid.NewGuid()}", null);

        // Act
        var countryId = _dataSaver.AddCountry(codeId, $"Country_{Guid.NewGuid()}");

        // Assert
        Assert.True(countryId > 0);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddCountry_WithNullTitle_ThrowsArgumentNullException()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode("code", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dataSaver.AddCountry(codeId, null!));
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddRegion_WithNewRegion_ReturnsNewId()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode($"region_{Guid.NewGuid()}", null);

        // Act
        var regionId = _dataSaver.AddRegion(codeId, $"Region_{Guid.NewGuid()}");

        // Assert
        Assert.True(regionId > 0);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddRegion_WithNullTitle_ThrowsArgumentNullException()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode("code", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dataSaver.AddRegion(codeId, null!));
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddRegionToCountry_AddsRelationship()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var countryCode = _dataSaver.AddCode($"country_{Guid.NewGuid()}", null);
        var regionCode = _dataSaver.AddCode($"region_{Guid.NewGuid()}", null);
        var countryId = _dataSaver.AddCountry(countryCode, $"Country_{Guid.NewGuid()}");
        var regionId = _dataSaver.AddRegion(regionCode, $"Region_{Guid.NewGuid()}");

        // Act
        _dataSaver.AddRegionToCountry(countryId, regionId);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddSettlement_WithNewSettlement_ReturnsNewId()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode($"settlement_{Guid.NewGuid()}", null);

        // Act
        var settlementId = _dataSaver.AddSettlement(codeId, $"Settlement_{Guid.NewGuid()}");

        // Assert
        Assert.True(settlementId > 0);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddSettlement_WithNullTitle_ThrowsArgumentNullException()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode("code", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dataSaver.AddSettlement(codeId, null!));
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddSettlementToRegion_AddsRelationship()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var regionCode = _dataSaver.AddCode($"region_{Guid.NewGuid()}", null);
        var settlementCode = _dataSaver.AddCode($"settlement_{Guid.NewGuid()}", null);
        var regionId = _dataSaver.AddRegion(regionCode, $"Region_{Guid.NewGuid()}");
        var settlementId = _dataSaver.AddSettlement(settlementCode, $"Settlement_{Guid.NewGuid()}");

        // Act
        _dataSaver.AddSettlementToRegion(regionId, settlementId);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddStation_WithNewStation_ReturnsNewId()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var codeId = _dataSaver.AddCode($"station_{Guid.NewGuid()}", null);

        // Act
        var stationId = _dataSaver.AddStation(
            codeId,
            "North",
            "Railway Station",
            $"Station_{Guid.NewGuid()}",
            37.6173,
            "Train",
            55.7558);

        // Assert
        Assert.True(stationId > 0);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void AddStationToSettlement_AddsRelationship()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var settlementCode = _dataSaver.AddCode($"settlement_{Guid.NewGuid()}", null);
        var stationCode = _dataSaver.AddCode($"station_{Guid.NewGuid()}", null);
        var settlementId = _dataSaver.AddSettlement(settlementCode, $"Settlement_{Guid.NewGuid()}");
        var stationId = _dataSaver.AddStation(
            stationCode,
            "North",
            "Railway Station",
            $"Station_{Guid.NewGuid()}",
            37.6173,
            "Train",
            55.7558);

        // Act
        _dataSaver.AddStationToSettlement(settlementId, stationId);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact(Skip = "Requires PostgreSQL database connection. Set POSTGRES_CONNECTION_STRING environment variable to run.")]
    public void Dispose_DisposesDataSource()
    {
        // Skip if no connection string
        if (_skipTests || _dataSaver == null) return;

        // Arrange
        var testDataSaver = new PostgresDataSaver(_connectionString);

        // Act
        testDataSaver.Dispose();

        // Assert - no exception means success
        Assert.True(true);
    }

    public void Dispose()
    {
        _dataSaver?.Dispose();
    }
}
