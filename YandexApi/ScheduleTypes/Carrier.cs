using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Carrier
{
    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public int? code { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string title { get; set; }

    [JsonProperty("codes", NullValueHandling = NullValueHandling.Ignore)]
    public Codes codes { get; set; }
}