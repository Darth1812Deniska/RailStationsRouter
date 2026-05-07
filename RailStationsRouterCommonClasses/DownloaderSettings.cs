namespace RailStationsRouterCommonClasses;

public class DownloaderSettings
{
    public string YandexRaspApiToken { get; set; }
    public string PostgresConnectionString { get; set; }
    public string SqliteDatabasePath { get; set; }
    /// <summary>
    /// Тип базы данных для сохранения ("SQLite" или "PostgreSQL")
    /// </summary>
    public string DatabaseType { get; set; } = "SQLite";
    
    public DownloaderSettings(string yandexRaspApiToken)
    {
        YandexRaspApiToken = yandexRaspApiToken;
    }
    public DownloaderSettings() { }

}