using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Station
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("popular_title")]
    public string PopularTitle { get; set; }

    [JsonProperty("short_title")]
    public string ShortTitle { get; set; }

    [JsonProperty("station_type")]
    public string StationType { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("transport_type")]
    public string TransportType { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}