using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using RailStationsRouterCommonClasses;
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
YandexApi yandexApi = new YandexApi(downloaderSettings.YandexRaspApiToken);
Root? root = yandexApi.GetStationsList();




NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
NpgsqlDataSource dataSource = dataSourceBuilder.Build();

if (root == null)
{
    return;
}

List<Country> countries = root.countries;

foreach (Country country in countries)
{
    var countryCode = country.codes;
    if (countryCode != null)
    {
        var countryCodeId = AddCodeToDataBase(countryCode);
        var countryId = AddCountryToDataBase(countryCodeId, country.title ?? string.Empty);
        List<Region>? regions = country.regions;
        if (regions != null)
        {
            foreach (Region region in regions)
            {
                var regionCode = region.codes;
                if (regionCode != null)
                {
                    var regionCodeId = AddCodeToDataBase(regionCode);
                    var regionId = AddRegionToDataBase(regionCodeId, region.title ?? string.Empty);
                    AddCountryRegionLinkToDataBase(countryId, regionId);
                    List<Settlement>? settlements = region.settlements;
                    if (settlements != null)
                    {
                        foreach (Settlement settlement in settlements)
                        {
                            var settlementCode = settlement.codes;
                            long settlementCodeId = AddCodeToDataBase(settlementCode);
                            long settlementId =
                                AddSettlementToDataBase(settlementCodeId, settlement.title ?? string.Empty);
                            AddRegionSettlementLinkToDataBase(regionId, settlementId);
                            foreach (Station station in settlement.stations)
                            {
                                Codes? codes = station.codes;
                                if (codes != null)
                                {
                                    long codeId = AddCodeToDataBase(codes);
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


                                    long stationId = AddStationToDataBase(codeId,
                                        station.direction,
                                        station.station_type,
                                        station.title,
                                        convLongitude,
                                        station.transport_type,
                                        convLatitude);
                                    AddSettlementStationLinkToDataBase(settlementId, stationId);
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

long AddCodeToDataBase(Codes codes)
{
    long result = 0;
    using (NpgsqlCommand command =
           dataSource.CreateCommand("SELECT public.rsr_f_add_code(:p_yandex_code, :p_esr_code);"))
    {
        NpgsqlParameter paramYandexCode = new NpgsqlParameter("p_yandex_code", codes.yandex_code ?? string.Empty);
        NpgsqlParameter paramEsrCode = new NpgsqlParameter("p_esr_code", codes.esr_code ?? string.Empty);
        command.Parameters.Add(paramYandexCode);
        command.Parameters.Add(paramEsrCode);
        object? rawResult = command.ExecuteScalarAsync().Result;
        if (rawResult != null)
        {
            result = (long)rawResult;
        }
    }

    return result;
}

long AddCountryToDataBase(long codeId, string title)
{
    long result = 0;

    using (NpgsqlCommand command =
           dataSource.CreateCommand("SELECT public.rsr_f_add_country(:p_codeid, :p_title);"))
    {
        NpgsqlParameter paramCodeId = new NpgsqlParameter("p_codeid", codeId);
        NpgsqlParameter paramTitle = new NpgsqlParameter("p_title", title);
        command.Parameters.Add(paramCodeId);
        command.Parameters.Add(paramTitle);
        object? rawResult = command.ExecuteScalarAsync().Result;
        if (rawResult != null)
        {
            result = (long)rawResult;
        }
    }

    return result;
}

long AddRegionToDataBase(long codeId, string title)
{
    long result = 0;

    using (NpgsqlCommand command =
           dataSource.CreateCommand("SELECT public.rsr_f_add_region(:p_code_id, :p_title);"))
    {
        NpgsqlParameter paramCodeId = new NpgsqlParameter("p_code_id", codeId);
        NpgsqlParameter paramTitle = new NpgsqlParameter("p_title", title);
        command.Parameters.Add(paramCodeId);
        command.Parameters.Add(paramTitle);
        object? rawResult = command.ExecuteScalarAsync().Result;
        if (rawResult != null)
        {
            result = (long)rawResult;
        }
    }

    return result;
}

void AddCountryRegionLinkToDataBase(long countryId, long regionId)
{
    using (NpgsqlCommand command =
           dataSource.CreateCommand("SELECT public.rsr_f_add_region_to_country(:p_country_id, :p_region_id);"))
    {
        NpgsqlParameter paramCountryId = new NpgsqlParameter("p_country_id", countryId);
        NpgsqlParameter paramRegionId = new NpgsqlParameter("p_region_id", regionId);
        command.Parameters.Add(paramCountryId);
        command.Parameters.Add(paramRegionId);
        command.ExecuteScalarAsync();
    }
}

long AddSettlementToDataBase(long codeId, string title)
{
    long result = 0;

    using (NpgsqlCommand command =
           dataSource.CreateCommand("SELECT public.rsr_f_add_settlement(:p_code_id, :p_title);"))
    {
        NpgsqlParameter paramCodeId = new NpgsqlParameter("p_code_id", codeId);
        NpgsqlParameter paramTitle = new NpgsqlParameter("p_title", title);
        command.Parameters.Add(paramCodeId);
        command.Parameters.Add(paramTitle);
        object? rawResult = command.ExecuteScalarAsync().Result;
        if (rawResult != null)
        {
            result = (long)rawResult;
        }
    }

    return result;
}

void AddRegionSettlementLinkToDataBase(long regionId, long settlementId)
{
    using (NpgsqlCommand command =
           dataSource.CreateCommand("SELECT public.rsr_f_add_settlement_to_region(:p_region_id, :p_settlement_id);"))
    {
        NpgsqlParameter paramRegionId = new NpgsqlParameter("p_region_id", regionId);
        NpgsqlParameter paramSettlementId = new NpgsqlParameter("p_settlement_id", settlementId);
        command.Parameters.Add(paramRegionId);
        command.Parameters.Add(paramSettlementId);
        command.ExecuteScalarAsync();
    }
}

long AddStationToDataBase(long codeId, string? direction, string? stationType, string? title, double? longitude,
    string? transportType, double? latitude)
{
    long result = 0;
    using (NpgsqlCommand command = dataSource.CreateCommand(
               "SELECT public.rsr_f_add_station(" +
               ":p_codeid, " +
               ":p_direction, " +
               ":p_station_type, " +
               ":p_title, " +
               ":p_longitude, " +
               ":p_transport_type, " +
               ":p_latitude);"))
    {
        NpgsqlParameter paramCodeId = new NpgsqlParameter("p_codeid", codeId);
        NpgsqlParameter paramDirection =
            new NpgsqlParameter("p_direction", string.IsNullOrEmpty(direction) ? DBNull.Value : direction);
        NpgsqlParameter paramStationType = new NpgsqlParameter("p_station_type",
            string.IsNullOrEmpty(stationType) ? DBNull.Value : stationType);
        NpgsqlParameter paramTitle = new NpgsqlParameter("p_title",
            string.IsNullOrEmpty(title) ? DBNull.Value : title);
        NpgsqlParameter paramLongitude =
            new NpgsqlParameter("p_longitude", longitude == null ? DBNull.Value : longitude);
        NpgsqlParameter paramTransportType = new NpgsqlParameter("p_transport_type",
            string.IsNullOrEmpty(transportType) ? DBNull.Value : transportType);
        NpgsqlParameter paramLatitude = new NpgsqlParameter("p_latitude",
            latitude == null ? DBNull.Value : latitude);
        command.Parameters.Add(paramCodeId);
        command.Parameters.Add(paramDirection);
        command.Parameters.Add(paramStationType);
        command.Parameters.Add(paramTitle);
        command.Parameters.Add(paramLongitude);
        command.Parameters.Add(paramTransportType);
        command.Parameters.Add(paramLatitude);
        var rawResult = command.ExecuteScalarAsync().Result;
        if (rawResult != null)
        {
            result = (long)rawResult;
        }
    }

    return result;
}

void AddSettlementStationLinkToDataBase(long settlementId, long stationId)
{
    using NpgsqlCommand command =
        dataSource.CreateCommand("SELECT public.rsr_f_add_station_to_settlement(:p_settlement_id, :p_station_id);");
    NpgsqlParameter paramSettlementId = new NpgsqlParameter("p_settlement_id", settlementId);
    NpgsqlParameter paramStationId = new NpgsqlParameter("p_station_id", stationId);
    command.Parameters.Add(paramSettlementId);
    command.Parameters.Add(paramStationId);
    command.ExecuteScalarAsync();
}