using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Station
{
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string type { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string title { get; set; }

    [JsonProperty("short_title", NullValueHandling = NullValueHandling.Ignore)]
    public string short_title { get; set; }

    [JsonProperty("popular_title", NullValueHandling = NullValueHandling.Ignore)]
    public string popular_title { get; set; }

    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public string code { get; set; }

    [JsonProperty("station_type", NullValueHandling = NullValueHandling.Ignore)]
    public string station_type { get; set; }

    [JsonProperty("station_type_name", NullValueHandling = NullValueHandling.Ignore)]
    public string station_type_name { get; set; }

    [JsonProperty("transport_type", NullValueHandling = NullValueHandling.Ignore)]
    public string transport_type { get; set; }
}