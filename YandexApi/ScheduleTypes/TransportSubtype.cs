using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class TransportSubtype
{
    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public object title { get; set; }

    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public object code { get; set; }

    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    public object color { get; set; }
}