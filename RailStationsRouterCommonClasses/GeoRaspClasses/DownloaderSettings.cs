namespace RailStationsRouterCommonClasses.GeoRaspClasses
{
    public class DownloaderSettings
    {
        public string YandexRaspApiToken { get; set; }
        public DownloaderSettings(string yandexRaspApiToken)
        {
            YandexRaspApiToken = yandexRaspApiToken;
        }
        public DownloaderSettings() { }

    }
}
