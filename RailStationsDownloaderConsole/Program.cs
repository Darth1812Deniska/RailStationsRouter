using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RailStationsRouterCommonClasses;
using RailStationsRouterCommonClasses.DB;
using RailStationsRouterCommonClasses.DB.PostgreSQL;
using RailStationsRouterCommonClasses.DB.SQLite;
using YandexRaspApi;
using YandexRaspApi.StationsListTypes;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
if (configuration == null)
    return;
DownloaderSettings? downloaderSettings = configuration.GetSection("DownloaderSettings").Get<DownloaderSettings>();
if (downloaderSettings == null)
{
    return;
}

Console.WriteLine(downloaderSettings.YandexRaspApiToken);

// Чтение настройки логгирования из appsettings.json
bool isLogging = configuration.GetValue<bool>("IsLogging");
Logger.IsLogging = isLogging;

YandexApi yandexApi = new YandexApi(downloaderSettings.YandexRaspApiToken);
StationsListRoot? root = yandexApi.GetStationsList();

if (root == null)
{
    return;
}

// Инициализация базы данных в зависимости от выбранного типа
IDataSaver dataSaver;
string dbType = downloaderSettings.DatabaseType?.ToLower() ?? "sqlite";

if (dbType == "postgresql" || dbType == "postgres")
{
    if (string.IsNullOrEmpty(downloaderSettings.PostgresConnectionString))
    {
        Console.WriteLine("Ошибка: строка подключения к PostgreSQL не указана в настройках");
        return;
    }
    
    Console.WriteLine("Инициализация PostgreSQL базы данных");
    try
    {
        var postgresDataSaver = new PostgresDataSaver(downloaderSettings.PostgresConnectionString);
        dataSaver = postgresDataSaver;
        Console.WriteLine("PostgreSQL база данных успешно инициализирована");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка инициализации PostgreSQL: {ex.Message}");
        return;
    }
}
else
{
    // По умолчанию используем SQLite
    Console.WriteLine($"Инициализация SQLite базы данных: {downloaderSettings.SqliteDatabasePath}");
    var sqliteDataSaver = new SqliteDataSaver(downloaderSettings.SqliteDatabasePath);
    sqliteDataSaver.Initialize();
    dataSaver = sqliteDataSaver;
    Console.WriteLine("SQLite база данных успешно инициализирована");
}

/*// Использование RailStationDownloader для сохранения данных
var railStationDownloader = new RailStationDownloader(dataSaver);
railStationDownloader.DownloadAndSaveStations(root);

Console.WriteLine("Загрузка данных завершена");
dataSaver.Dispose();*/

var scheduleList = yandexApi.GetScheduleList("s2000005");
foreach (var schedule in scheduleList.schedule)
{
    var thread = schedule.thread;
    Console.WriteLine(thread.number);
    Console.WriteLine(thread.title);
}
Console.WriteLine(scheduleList.date); 
Console.WriteLine(scheduleList.@event);


