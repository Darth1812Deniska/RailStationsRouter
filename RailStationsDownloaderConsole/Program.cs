using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RailStationsRouterCommonClasses.GeoRaspClasses;
using YandexRaspApi;

var configuration = new ConfigurationBuilder()
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
var root = yandexApi.GetStationsList();
//Console.WriteLine(jsonResult);
//jsonResult=jsonResult.Replace("\"\"", "null");

var countries = root.countries;
foreach (Country country in countries)
{
    if (country.title.ToLower() == "Россия".ToLower())
    {
        var regions = country.regions;
        foreach (var region in regions)
        {
            if (region.title.ToLower() == "Липецкая область".ToLower())
            {
                Console.WriteLine(region.title);
                var settlements = region.settlements;
                foreach (var settlement in settlements)
                {
                    Console.WriteLine(settlement.title);
                    foreach (var station in settlement.stations)
                    {
                        Console.WriteLine($"\t-{station.title}");
                    }
                }
            }
        }
    }
}