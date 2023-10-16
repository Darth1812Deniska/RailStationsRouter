
using Microsoft.Extensions.Configuration;
using RailStationsRouterCommonClasses.GeoRaspClasses;
using YandexRaspApi;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
if (configuration == null)
    return;
DownloaderSettings downloaderSettings = configuration.GetSection("DownloaderSettings").Get<DownloaderSettings>();
if (downloaderSettings == null)
{
    return;
}
Console.WriteLine(downloaderSettings.YandexRaspApiToken);
YandexApi yandexApi = new YandexApi(downloaderSettings.YandexRaspApiToken);
string jsonResult = yandexApi.GetStationsList();
//Console.WriteLine(jsonResult);
File.WriteAllText("result.json", jsonResult);