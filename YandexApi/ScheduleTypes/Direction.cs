using Newtonsoft.Json;

namespace YandexRaspApi.ScheduleTypes;

public class Direction
{
    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public string code { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string title { get; set; }
}