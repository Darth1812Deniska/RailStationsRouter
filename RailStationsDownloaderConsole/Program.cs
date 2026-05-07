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
Root? root = yandexApi.GetStationsList();

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

List<Country> countries = root.countries;

foreach (Country country in countries)
{
    var countryCode = country.codes;
    if (countryCode != null)
    {
        // Сохранение данных в выбранную БД
        var countryCodeId = dataSaver.AddCode(countryCode.yandex_code, countryCode.esr_code);
        var countryId = dataSaver.AddCountry(countryCodeId, country.title ?? string.Empty);
        
        List<Region>? regions = country.regions;
        if (regions != null)
        {
            foreach (Region region in regions)
            {
                var regionCode = region.codes;
                if (regionCode != null)
                {
                    // Сохранение региона
                    var regionCodeId = dataSaver.AddCode(regionCode.yandex_code, regionCode.esr_code);
                    var regionId = dataSaver.AddRegion(regionCodeId, region.title ?? string.Empty);
                    dataSaver.AddRegionToCountry(countryId, regionId);
                    
                    List<Settlement>? settlements = region.settlements;
                    if (settlements != null)
                    {
                        foreach (Settlement settlement in settlements)
                        {
                            var settlementCode = settlement.codes;
                            
                            // Сохранение поселения
                            long settlementCodeId = dataSaver.AddCode(settlementCode.yandex_code, settlementCode.esr_code);
                            long settlementId = dataSaver.AddSettlement(settlementCodeId, settlement.title ?? string.Empty);
                            dataSaver.AddSettlementToRegion(regionId, settlementId);
                            
                            foreach (Station station in settlement.stations)
                            {
                                Codes? codes = station.codes;
                                if (codes != null)
                                {
                                    double? convLongitude = null;
                                    if (station.longitude is JsonElement
                                        {
                                            ValueKind: JsonValueKind.Number
                                        } jsLongitude)
                                    {
                                        convLongitude = jsLongitude.GetDouble();
                                    }

                                    double? convLatitude = null;
                                    if (station.latitude is JsonElement { ValueKind: JsonValueKind.Number } jsLatitude)
                                    {
                                        convLatitude = jsLatitude.GetDouble();
                                    }

                                    // Сохранение станции
                                    long codeId = dataSaver.AddCode(codes.yandex_code, codes.esr_code);
                                    long stationId = dataSaver.AddStation(codeId,
                                        station.direction,
                                        station.station_type,
                                        station.title,
                                        convLongitude,
                                        station.transport_type,
                                        convLatitude);
                                    dataSaver.AddStationToSettlement(settlementId, stationId);
                                    
                                    string fullStationText = $"Страна:{country.title}, " +
                                                             $"Регион: {region.title}, " +
                                                             $"Поселение: {settlement.title}, " +
                                                             $"Станция: {station.title}";
                                    Console.WriteLine(fullStationText);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

Console.WriteLine("Загрузка данных завершена");
dataSaver.Dispose();