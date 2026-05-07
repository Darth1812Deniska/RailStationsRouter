using RailStationsRouterCommonClasses;
using Xunit;

namespace RailStationsRouterCommonClasses.Tests;

public class DownloaderSettingsTests
{
    [Fact]
    public void Constructor_WithToken_SetsYandexRaspApiToken()
    {
        // Arrange
        var token = "test-api-token-123";

        // Act
        var settings = new DownloaderSettings(token);

        // Assert
        Assert.Equal(token, settings.YandexRaspApiToken);
    }

    [Fact]
    public void Constructor_Default_SetsDefaultValues()
    {
        // Act
        var settings = new DownloaderSettings();

        // Assert
        Assert.Null(settings.YandexRaspApiToken);
        Assert.Null(settings.PostgresConnectionString);
        Assert.Null(settings.SqliteDatabasePath);
        Assert.Equal("SQLite", settings.DatabaseType);
    }

    [Fact]
    public void Constructor_WithToken_DoesNotSetOtherProperties()
    {
        // Arrange
        var token = "test-token";

        // Act
        var settings = new DownloaderSettings(token);

        // Assert
        Assert.Null(settings.PostgresConnectionString);
        Assert.Null(settings.SqliteDatabasePath);
        Assert.Equal("SQLite", settings.DatabaseType);
    }

    [Fact]
    public void DatabaseType_CanBeModified()
    {
        // Arrange
        var settings = new DownloaderSettings();

        // Act
        settings.DatabaseType = "PostgreSQL";

        // Assert
        Assert.Equal("PostgreSQL", settings.DatabaseType);
    }

    [Fact]
    public void PostgresConnectionString_CanBeSet()
    {
        // Arrange
        var settings = new DownloaderSettings();
        var connectionString = "Host=localhost;Database=testdb;Username=user;Password=pass";

        // Act
        settings.PostgresConnectionString = connectionString;

        // Assert
        Assert.Equal(connectionString, settings.PostgresConnectionString);
    }

    [Fact]
    public void SqliteDatabasePath_CanBeSet()
    {
        // Arrange
        var settings = new DownloaderSettings();
        var dbPath = "/path/to/database.db";

        // Act
        settings.SqliteDatabasePath = dbPath;

        // Assert
        Assert.Equal(dbPath, settings.SqliteDatabasePath);
    }
}
